$.get("/api/app/update/available", function(data) {
    if (data.update_available) {
        var link = $('.nav > li > a[href="/dashboard/app/softwareupdates"]');
        link.append('<span class="badge badge-pill badge-success">New</span>');
    }
});
