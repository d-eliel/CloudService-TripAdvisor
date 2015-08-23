/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */

var url = "http://localhost:8662"; // local
//var url = "tripadvisor-204280036.cloudapp.net"; // server

/* functions executions */
// when document ready
$(document).ready(function () {
    var data;

    $.ajax({
        url: url + '/Stats/GetStats',
        type: "GET",
        success: function (result) {
            console.log(result);
            data = result;
            graph(data[0], "#city-stats");
            graph(data[1], "#country-stats");
            graph(data[2], "#type-stats");
            graph(data[3], "#date-stats");
        }
    });

});


function graph(data, id) {
    //Width and height
    var w = 500;
    var h = 75;

    //Data
    var dataset = data;
    console.log(data);

    //Create SVG element
    var svg = d3.select(id)
                .append("svg")
                .attr("width", w)
                .attr("height", h)
                .attr("overflow-y", "visible")
                .attr("overflow-x", "auto")
                .attr("class", "row");

    var circles = svg.selectAll("circle")
        .data(dataset)
        .enter()
        .append("circle");

    circles.attr("cx", function (d, i) {
        return (i * 50) + 25;
    })
    .attr("cy", h / 2)
    .attr("r", function (d) {
        return d.length * 10;
    })
    .attr("fill", "yellow")
    .attr("stroke", "orange")
    .attr("stroke-width", function(d) {
        return d.length/2;
    })
    .attr("class", "col-xs-1");

    svg.selectAll("text")
    .data(dataset)
    .enter()
    .append("text")
    .text(function (d) {
        return d[0];
    })
    .attr("font-size", "12px")
    .attr("fill", "red")
    .attr("class", "col-xs-1");
}