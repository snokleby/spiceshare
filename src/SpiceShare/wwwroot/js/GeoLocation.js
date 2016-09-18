if (!Date.now) {
    Date.now = function now() {
        return new Date().getTime();
    };
}

var spiceGeo = function () {
    function getById(id) {
        return document.getElementById(id);
    }
        if ("geolocation" in navigator) {
            var elm = getById('jslocation');
            if (elm !== null && elm !== undefined) {
                elm.className = '';
            }
        } else {
            showGeoFallBack();
        }
   
    function showGeoFallBack() {
        var fallbackdivs = document.querySelectorAll('.posfallback');
        for (var i = 0; i < fallbackdivs.length; ++i) {
            fallbackdivs[i].className = '';
        }
    }

    var cachedLat = null;
    var cachedLon = null;
    var cachePosTime = null;

    function GetGeoLocIntoFields(callBack, failcallBack) {
        if (cachedLat !== null && cachedLon !== null && cachePosTime !== null && (Date.now() - cachePosTime) / 1000 < 120) {
            getById('lat').value = cachedLat;
            getById('lon').value = cachedLon;
            var locationButton = getById('locationaction');
            if (locationButton !== null) {
                locationButton.style.display = '';
            }
            callBack();
            return;
        }
        var alreadyDone = false;
        var dotTimer = null;
        var geoWait = getById('geowait');
        setTimeout(function () {
            if (!alreadyDone) {
               
                geoWait.style.display = '';
                geoWait.textContent = "Finding your location. Remember to accept if your device asks for permision to share your location.";
                dotTimer = setInterval(function () {
                    geoWait.textContent = geoWait.textContent + ".";
                }, 350);
                
            }
        }, 300);

        function clearGeoWait() {
            geoWait.style.display = 'none';
            geoWait.textContent = "";
            clearInterval(dotTimer);

        }

        navigator.geolocation.getCurrentPosition(function (position) {
            alreadyDone = true;
            clearGeoWait();
            getById('lat').value = position.coords.latitude;
            getById('lon').value = position.coords.longitude;
            var locationButton = getById('locationaction');
            if (locationButton !== null) {
                locationButton.style.display = '';
            }
            callBack();
            cachedLat = position.coords.latitude;
            cachedLon = position.coords.longitude;
            cachePosTime = Date.now();

        }, function () {
            clearGeoWait();
            getById('geoerror').style.display = '';
            showGeoFallBack();
            if (failcallBack !== undefined) {
                failcallBack();
            }
        }, {
            enableHighAccuracy: false,
            timeout: 5000,
            maximumAge: Infinity
        });

    };

    return {
        GetGeoLocIntoFields: GetGeoLocIntoFields
     }
}();
