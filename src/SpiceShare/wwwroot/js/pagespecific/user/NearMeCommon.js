function executeSend(request, failfunct, newUrl, successFunct) {
    try {
        history.replaceState(null, "", newUrl);
    } catch (ex) { }
    request.send();
    request.onreadystatechange = function () {
        if (request.readyState === 4) { 
            if (request.status === 200) {
                document.getElementById('userslist').innerHTML = request.responseText;
                successFunct();
            } else {
                failfunct();
            }
        }
    };
};

(function () {
    var placeHolders = ["green pepper", "green chili", "pepper", "cinnamon", "rosemary", "curry"];
    var i = 0;
    setInterval(function () {
        var val = placeHolders[i];
        i += 1;
        if (i >= placeHolders.length) {
            i = 0;
        }
        document.getElementById("search").setAttribute("placeholder", val);
    }, 2000);
})();