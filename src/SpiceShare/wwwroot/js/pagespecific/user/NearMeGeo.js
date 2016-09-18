
var lastForm = '';

function FindUsersNear(startCallback, doneCallBack) {
    var request = new XMLHttpRequest();
    var lat = document.getElementById('lat').value;
    var lon = document.getElementById('lon').value;
    var search = document.getElementById('search').value;
    var newForm = lat + lon + search;
    if (newForm === lastForm || search.length === 0) {
        doneCallBack();
        return;
    }
    lastForm = newForm;
    startCallback();
    request.open('GET', "/api/UsersNearMe?lat=" + lat + "&lon=" + lon + "&search=" + search, true);
    var errElm = document.getElementById("erfld");
    // request.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');
    executeSend(request, function () {
        errElm.textContent = "Ooops, seems like automatic search failed. Check your internet-connection and click the button to search manually."
        errElm.style.display = "";
        document.getElementById("subbutton").className = "";
    }, "/User/NearMeGeo?lat=" + lat + "&lon=" + lon + "&search=" + search, function () {
        errElm.textContent = "";
        errElm.style.display = "none";
        doneCallBack();
    });
};

if (document.addEventListener) {
    document.getElementById('find').addEventListener('click', function (e) {
        FindUsersByZip();
        e.preventDefault();
    });
} else {
    document.getElementById('subbutton').className = '';
}

var geoIsUnavailable = false;

function DoAutoSearch(startCallback, doneCallBack) {
    if (geoIsUnavailable) {
        FindUsersNear(startCallback,function () {
            //    document.getElementById('distanceerror').style.display = '';
            doneCallBack();
        });

        return;
    }

    spiceGeo.GetGeoLocIntoFields(function () {
        FindUsersNear(startCallback,function() {
            //    document.getElementById('distanceerror').style.display = '';
            doneCallBack();
        })
    }, function () {
        geoIsUnavailable = true;
        FindUsersNear(startCallback,function() {
                //    document.getElementById('distanceerror').style.display = '';
                doneCallBack();
        });
        });
   
};