/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */

/*  AdviceDate input element is disabled (client can't change it).
    this script remove the disable attr so we will be able to submit it. */
$(function ($) {

    $('form').bind('submit', function () {
        $(this).find(':input').removeAttr('disabled');
    });

});