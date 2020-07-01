using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using JaneStreetAV.Models.Config;
using UX.Lib2.Cloud.Logger;

namespace JaneStreetAV.Devices
{
    public class DmSwitcher : DmSwitcherBase
   { 
        public DmSwitcher(ControlSystem controlSystem, SwitcherConfig switcherConfig)
        {
            ControlSystem = controlSystem;
            var assembly = Assembly.Load(typeof(DmMd32x32Cpu3).AssemblyName());
#if DEBUG
            CrestronConsole.PrintLine("DMSwitcher - Creating instance of {0}", switcherConfig.FrameType);
#endif
            try
            {
                var frameType = assembly.GetType(switcherConfig.FrameType).GetCType();
                var frameCtor = frameType.GetConstructor(new CType[] {typeof (uint), typeof (CrestronControlSystem)});
                Chassis = (Switch) frameCtor.Invoke(new object[] {switcherConfig.Id, controlSystem});
                for (var i = 1U; i <= Chassis.NumberOfOutputs; i++)
                {
                    _outInit[i] = false;
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error loading DM Frame type");
                throw e;
            }

            foreach (var cardConfig in switcherConfig.InputCards)
            {
                CardDevice card = null;
                try
                {
                    var cardType = assembly.GetType(cardConfig.Type).GetCType();
                    var ctor = cardType.GetConstructor(new CType[] { typeof(uint), typeof(Switch) });
                    card = (CardDevice)ctor.Invoke(new object[] { cardConfig.Number, Chassis });
                }
                catch (Exception e)
                {
                    CloudLog.Error("Could not create DM Input Card {0} with ID {1}, {2}", cardConfig.Type,
                        cardConfig.Number, e.Message);
                }

                if (card == null) continue;

                try
                {
                    var inputConfig =
                        switcherConfig.Inputs.FirstOrDefault(i => i.Number == card.SwitcherInputOutput.Number);
                    if (inputConfig == null) continue;
                    if (string.IsNullOrEmpty(inputConfig.EndpointType)) continue;
                    var endpointType = assembly.GetType(inputConfig.EndpointType).GetCType();
                    var ctor = endpointType.GetConstructor(new CType[] { typeof(DMInput) });
                    var endpoint = (DMEndpointBase)ctor.Invoke(new object[] { card.SwitcherInputOutput as DMInput });
                    endpoint.Description = inputConfig.Name;
                }
                catch (Exception e)
                {
                    CloudLog.Error("Could not create DM Input {0}, {1}", cardConfig.Number, e.Message);
                }
            }

            foreach (var cardConfig in switcherConfig.OutputCards)
            {
                DmcOutputSingle card = null;
                try
                {
                    var bladeType = assembly.GetType(cardConfig.Type).GetCType();
                    var ctor = bladeType.GetConstructor(new CType[] { typeof(uint), typeof(Switch) });
                    card = (DmcOutputSingle)ctor.Invoke(new object[] { cardConfig.Number, Chassis });
                }
                catch (Exception e)
                {
                    CloudLog.Error("Could not create DM Output Card {0} with ID {1}, {2}", cardConfig.Type,
                        cardConfig.Number, e.Message);
                }

                if (card == null) continue;

                for (var cardOutput = 1U; cardOutput <= 2; cardOutput++)
                {
                    var propName = "Card" + cardOutput;
                    var cardSingle =
                        (CardDevice)
                            card.GetCType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .First(p => p.Name == propName)
                                .GetValue(card);

                    try
                    {
                        var outputConfig =
                            switcherConfig.Outputs.FirstOrDefault(i => i.Number == cardSingle.SwitcherInputOutput.Number);
                        if (outputConfig == null) continue;
                        if (string.IsNullOrEmpty(outputConfig.EndpointType)) continue;
                        var endpointType = assembly.GetType(outputConfig.EndpointType).GetCType();
                        var ctor = endpointType.GetConstructor(new CType[] {typeof (DMOutput)});
                        var endpoint =
                            (DMEndpointBase) ctor.Invoke(new object[] {cardSingle.SwitcherInputOutput as DMOutput});
                        endpoint.Description = outputConfig.Name;
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Could not create DM Output {0}, {1}", cardSingle.SwitcherInputOutput.Number,
                            e.Message);
                    }
                }
            }

            Chassis.OnlineStatusChange += Chassis_OnlineStatusChange;
            Chassis.DMInputChange += OnDmInputChange;
            Chassis.DMOutputChange += OnDmOutputChange;

            try
            {
                var regResult = Chassis.Register();
                if (regResult != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    CloudLog.Error("Error registering DM Frame, result = {0}", regResult);
                }
            }
            catch (Exception e)
            {
                CloudLog.Error("Error trying to register DM Frame, {0}", e.Message);
            }

            try
            {
                foreach (var inputConfig in switcherConfig.Inputs)
                {
                    if (inputConfig.Number == 0 || inputConfig.Number > Chassis.NumberOfInputs) continue;
                    Chassis.Inputs[inputConfig.Number].Name.StringValue = inputConfig.Name;
                }

                foreach (var outputConfig in switcherConfig.Outputs)
                {
                    if (outputConfig.Number == 0 || outputConfig.Number > Chassis.NumberOfOutputs) continue;

                    Chassis.Outputs[outputConfig.Number].Name.StringValue = outputConfig.Name;
                }
            }
            catch (Exception e)
            {
                CloudLog.Error("Error setting names on DM switcher, {0}", e.Message);
            }
        }
    }
}