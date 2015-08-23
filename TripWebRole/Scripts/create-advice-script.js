/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */

/* functions executions */
// when document ready
$(document).ready(function () {
    getLocation();
    showDate();
});

/* functions definitions */

function getLocation () {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
    } else {
        $("#geoLocation").val("Geolocation is not supported by this browser");
    }
}


/* change the input attributes value by current location */
/* AJAX call to google api - change geoLocation to Address */

function showPosition (position) {

    $("#geoLocation").val(position.coords.latitude + "," + position.coords.longitude);

    var reqStr = ("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + (position.coords.latitude + "," + position.coords.longitude));

    $.ajax({
        url: reqStr,
        context: document.body
    }).done(function (res) {
        if (res.status == "OK") {
            console.log(res);
            res = res.results;

            var city = res[0].address_components[2].long_name;
            var lastComponent = res[0].address_components.length - 1;
            var country = res[0].address_components[lastComponent].long_name;

            $("#city").val(city);
            $("#country").val(country);
        }
    });
}

function showDate() {
    // set date format 
    var dateString = new Date();
    $("#adviceDate").val((dateString.getMonth() + 1) + "/" + dateString.getDate() + "/" + dateString.getFullYear());
}

/*  AdviceDate input element is disabled (client can't change it).
    this script remove the disable attr so we will be able to submit it when client click save/create. */
$(function ($) {

    $('form').bind('submit', function () {
        $(this).find(':input').removeAttr('disabled');
    });

});