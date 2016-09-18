(function() {
    function getById(id) {
        return document.getElementById(id);
    }
    if (window.location.protocol !== 'https:') {
        var warn = getById("hw");
        warn.appendChild(document.createTextNode("It looks like you're accessing this site on a non-secure connection. Some features will not be accessible. Please"));
        var link = document.createElement("a");
        link.href = "#";
        link.id = "tohttps";
        link.innerText = "use the https version";
        warn.appendChild(link);
        warn.appendChild(document.createTextNode(" for the full experience"));
        warn.style.display = '';
    }

    function setHttps(e) {
        e.preventDefault();
        window.location.href = "https:" + window.location.href.substring(window.location.protocol.length);
    }

    try {
        if (document.addEventListener) {
            getById("tohttps").addEventListener('click', function (e) {
                setHttps(e);
            });
        } else {
            getById("tohttps").attachEvent('click', function (e) {
                setHttps(e);
            });
        }
    } catch (ex) { }

    try {
        var userPageKey = "userpage";
        var setter = getById("privatelinksetter");
        var timeExpires = getById("timeexpires").value;
        if (setter !== null) {
            localStorage.setItem(userPageKey, JSON.stringify({ id: setter.dataset.identity, expires: timeExpires }));
        }
        var userIdenitity = JSON.parse(localStorage.getItem(userPageKey));
        var timenow = getById("timenow").value;
        if (userIdenitity !== null) {
            if (userIdenitity.expires <= timenow) {
                localStorage.removeItem(userPageKey);
            } else {
                var area = getById("privlinkph");
                var link = document.createElement("a");
                link.href = "/User/PrivateUserPage?identity=" + userIdenitity.id;
                link.innerText = "Go to your profile";
                area.appendChild(link);
            }
        }
    } catch (ex) {

    }

    setInterval(function() {
        var mE = getById("resetmin");
        var sE = getById("resetsec");

        var minutes = parseInt(mE.textContent);
        var seconds = parseInt(sE.textContent);
        if (seconds > 1) {
            seconds -= 1;
        } else {
            if (minutes > 0) {
                minutes = minutes - 1;
                seconds = 59;
            } else {
                minutes = 0;
                seconds = 0;
            }
        }
        mE.textContent = minutes;
        sE.textContent = seconds;
    }, 1000);

})();