using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Blades;
using Crestron.SimplSharpPro.DM.Endpoints;
using JaneStreetAV.Models.Config;
using UX.Lib2.Cloud.Logger;

namespace JaneStreetAV.Devices
{
    public class BigDmSwitcher : DmSwitcherBase
   { 
        public BigDmSwitcher(ControlSystem controlSystem, SwitcherConfig switcherConfig)
        {
            ControlSystem = controlSystem;
            var assembly = Assembly.Load(typeof(BladeSwitch).AssemblyName());
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
                CloudLog.Error("Error loading DM Frame type");
                throw e;
            }

            foreach (var bladeConfig in switcherConfig.InputBlades)
            {
                DmInputBlade blade = null;
                try
                {
                    var bladeType = assembly.GetType(bladeConfig.Type).GetCType();
                    var ctor = bladeType.GetConstructor(new CType[] {typeof (uint), typeof (Switch)});
                    blade = (DmInputBlade) ctor.Invoke(new object[] {bladeConfig.Number, Chassis});
                }
                catch (Exception e)
                {
                    CloudLog.Error("Could not create DM Input Blade {0} with ID {1}, {2}", bladeConfig.Type, bladeConfig.Number, e.Message);
                }

                if (blade == null) continue;

                for (var bladeInput = 1U; bladeInput <= 8; bladeInput++)
                {
                    var inputNumber = ((blade.Number - 1)*8) + bladeInput;

                    try
                    {
                        var inputConfig = switcherConfig.Inputs.FirstOrDefault(i => i.Number == inputNumber);
                        if (inputConfig == null) continue;
                        if (string.IsNullOrEmpty(inputConfig.EndpointType)) continue;
                        var endpointType = assembly.GetType(inputConfig.EndpointType).GetCType();
                        var ctor = endpointType.GetConstructor(new CType[] {typeof (DMInput)});
                        var endpoint = (DMEndpointBase) ctor.Invoke(new object[] {blade.Inputs[bladeInput]});
                        endpoint.Description = inputConfig.Name;
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Could not create DM Input {0}, {1}", inputNumber, e.Message);
                    }
                }
            }

            foreach (var bladeConfig in switcherConfig.OutputBlades)
            {
                DmOutputBlade blade = null;
                try
                {
                    var bladeType = assembly.GetType(bladeConfig.Type).GetCType();
                    var ctor = bladeType.GetConstructor(new CType[] { typeof(uint), typeof(Switch) });
                    blade = (DmOutputBlade)ctor.Invoke(new object[] { bladeConfig.Number, Chassis });
                }
                catch (Exception e)
                {
                    CloudLog.Error("Could not create DM Output Blade {0} with ID {1}, {2}", bladeConfig.Type, bladeConfig.Number, e.Message);
                }

                if (blade == null) continue;

                for (var bladeOutput = 1U; bladeOutput <= 8; bladeOutput++)
                {
                    var outputNumber = ((blade.Number - 1)*8) + bladeOutput;

                    try
                    {
                        var outputConfig = switcherConfig.Outputs.FirstOrDefault(i => i.Number == outputNumber);
                        if (outputConfig == null) continue;
                        if (string.IsNullOrEmpty(outputConfig.EndpointType)) continue;
                        var endpointType = assembly.GetType(outputConfig.EndpointType).GetCType();
                        var ctor = endpointType.GetConstructor(new CType[] {typeof (DMOutput)});
                        var endpoint = (DMEndpointBase) ctor.Invoke(new object[] {blade.Outputs[bladeOutput]});
                        endpoint.Description = outputConfig.Name;
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Could not create DM Output {0}, {1}", outputNumber, e.Message);
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