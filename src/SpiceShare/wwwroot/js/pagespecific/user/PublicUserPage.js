(function() {
    function GetDistanceToUser() {
        var lat = document.getElementById('lat').value;
        var lon = document.getElementById('lon').value;
        var otherUserId = document.getElementById('userid').value;
        var request = new XMLHttpRequest();
        request.open('GET', "/api/distanceToUser?id=" + otherUserId + "&lat=" + lat + "&lon=" + lon, true);
        // request.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');
        request.send();
        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                var errElm = document.getElementById('distajaxerr');

                if (request.status === 200) {
                    document.getElementById('distancetext').style.display = '';
                    var elm = document.getElementById('distance');
                    elm.textContent = request.responseText;
                    elm.style.display = '';
                    errElm.style.display = 'none';
                } else {
                    errElm.style.display = '';
                    errElm.textContent = "Oops, we are unable to connect to the server. Check your internet-connection and try again.";
                }
            }

        };
    }

    document.getElementById('locationaction').addEventListener('click', function (e) {
        GetDistanceToUser();
        e.preventDefault();
    });

document.getElementById('updateuserloc').addEventListener('click', function (e) {
    spiceGeo.GetGeoLocIntoFields(function () {
        GetDistanceToUser();
    });
    e.preventDefault();
});
})();
