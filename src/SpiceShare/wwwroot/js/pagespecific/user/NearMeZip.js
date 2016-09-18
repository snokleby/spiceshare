
var lastForm = '';

function FindUsersByZip(startCallback,doneCallBack) {
    var request = new XMLHttpRequest();
    var country = document.getElementById('country').value;
    var zip = document.getElementById('zipcode').value;
    var search = document.getElementById('search').value;
    var newForm = country + zip + search;
    if (newForm === lastForm || search.length === 0) {
        doneCallBack();
        return;
    }
    lastForm = newForm;
    startCallback();
    var errElm = document.getElementById("erfld");

    request.open('GET', "/api/UsersNearZip?country=" + country + "&zipcode=" + zip + "&search=" + search, true);
    // request.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');
    executeSend(request, function () {
        errElm.textContent = "Ooops, seems like automatic search failed. Check your internet-connection and click the button to search manually.";
        errElm.style.display = "";
        document.getElementById("subbutton").className = "";
    }, "/User/NearMeZip?country=" + country + "&zipcode=" + zip + "&search=" + search, function () {
        errElm.textContent = "";
        errElm.style.display = "none";
        doneCallBack();
    });
};

if (document.addEventListener) {
    document.getElementById('find').addEventListener('click', function(e) {
        FindUsersByZip();
        e.preventDefault();
    });
} else {
    document.getElementById('subbutton').className = '';
}

function DoAutoSearch(startCallback, doneCallBack) {
    FindUsersByZip(startCallback, function () {
        doneCallBack();
    });
};