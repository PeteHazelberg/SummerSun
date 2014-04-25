

function InitChart() {
    // TO DO I don't like the hidden field approach to pass data from the view
    // I need an alternative

    // Let's try a promise and make a couple of parallel calls
    // For now let's call the default samples endpoint without a startdate 
    $.when(getPxResource($('#pointUrl').val()), getPxResource($('#pointUrl').val() + '/samples'))
        .done(function (point, samples) {
            showChart(point[0], samples[0])
    })
}

function showChart(point, data){
    var samples = [];
    for (var i in data.items)
        samples.push([Date.parse(data.items[i].ts), data.items[i].val]);
   drawChart(point, samples)
}

function setHeader(xhr) {
    var token = $('#AccessToken').val()
    xhr.setRequestHeader('Authorization', 'Bearer ' + token);
}

function getPxResource(resourceUrl, callback) {
   return  $.ajax({
            type: "GET",
            dataType: 'json',
            url: resourceUrl,
            crossDomain: true,
            headers: { 'Authorization': 'Bearer ' + $('#AccessToken').val() },
        })
      .done(function (data) {
          if (callback)
            callback(data);
      })
      .fail(function (xhr, textStatus, errorThrown) {
          console.debug(xhr.responseText);
          console.debug(textStatus);
      });
    
}

function drawChart(point, data) {
    $('#chartContainer').highcharts({
        chart: {
            zoomType: 'x',
            type: 'line'
        },
        title: {
            text: 'Single Point Chart (zoom)'
        },
        xAxis: {
            type: 'datetime',
            dateTimeLabelFormats: {
                second: '%Y-%m-%d<br/>%H:%M:%S',
                minute: '%Y-%m-%d<br/>%H:%M',
                hour: '%Y-%m-%d<br/>%H:%M',
                day: '%Y<br/>%m-%d',
                week: '%Y<br/>%m-%d',
                month: '%Y-%m',
                year: '%Y'
            }
        },
        yAxis: {
            title: {
                text: point.unit ? point.unit : point.state
            }
        },
        series: [{
            name: point.name,
            data: data 
        }]
    });
}

