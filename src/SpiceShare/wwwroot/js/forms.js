if (!Date.now) {
    Date.now = function now() {
        return new Date().getTime();
    };
}

var spiceforms = (function () {
    //fallback
    if (!document.addEventListener) {
        document.getElementById("submit").className = '';
    }

    function getByQuerySelector(selector) {
        return document.querySelector(selector);
    }
    function getById(id) {
        return document.getElementById(id);
    }
    var inputs = document.querySelectorAll('input,textarea');

    function getFormStr() {
        var formStr = "";

        [].forEach.call(inputs, function (elm) {
            if (elm.value !== null) {
                formStr += elm.value;
            } else {
                formStr += elm.innerText;
            }
        });
        return formStr;
    }

    var lastPress = Date.now();
    var dirty = false;
    var persistInAction = false;
    var valid = false;
    var full = false;
    var lastTouchedField;
    var lastFormStr = getFormStr();
    var contentHasChangedSinceValidNess = false;

    var postUrl;

    if (getById("validfromserver") !== null) {
        valid = true;
    }

    if (getById("completefromserver") !== null) {
        full = true;
    }

    function setnodeBackgroundColor(elm, col) {
        elm.style.background = col;
    }

    function postUserFormAjax() {
        persistInAction = true;
        lastFormStr = getFormStr();
        dirty = false;
        var formElement = getByQuerySelector("form");
        var request = new XMLHttpRequest();
        request.open("POST", postUrl);
        try {
            request.send(new FormData(formElement));
        } catch (ex) {
            getById("submit").className = '';
        }
        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                persistInAction = false;
                valid = false;
                full = false;
                var ne = getById("netwerr");
                if (request.status !== 200) {
                    getById("submit").className = "";
                    var resTxt = request.responseText;
                    if (resTxt !== null && resTxt !== "") {
                        ne.textContent = resTxt;
                    } else {
                        ne.textContent = "Ooops, we were unable to automatically save your changes. Check your internet connection and click the save-button to try again.";
                    }
                    ne.style.display = "";
                    return;
                }
                ne.style.display = "none";
                var obj = JSON.parse(request.responseText);

         
                if (obj.status === "ok") {
                    valid = true;
                } else if (obj.status === "full") {
                    full = true;
                } else if (obj.status === "incomplete") {
                }
                var anythingChanged = false;
                try {
                    obj.fields.forEach(function (elm) {
                        var domElm = getById(elm.id);
                        var parentElm = domElm.parentNode;
                        var val = getById(elm.id + "_val");
                        if (!elm.valid) {
                            if (domElm === lastTouchedField) {
                                domElm.setAttribute("aria-invalid", "true");
                                setnodeBackgroundColor(parentElm, "#F8D3FF");
                                if (val !== null)
                                    val.style.display = "";
                            }
                        } else {
                            setnodeBackgroundColor(parentElm, "");
                            domElm.removeAttribute("aria-invalid");

                            if (val !== null)
                                val.style.display = "none";
                        }
                        if (elm.changed && elm.valid) {
                            anythingChanged = true;
                            var changedIndicator = getById(elm.id + "_sh");
                            changedIndicator.textContent = "saved.";
                            setnodeBackgroundColor(parentElm,"#b8ffed");
                            changedIndicator.dataset.changedguid = elm.changedGUid;
                            setTimeout(function () {
                                if (changedIndicator.dataset.changedguid === elm.changedGUid) {
                                    changedIndicator.textContent = "";
                                    setnodeBackgroundColor(parentElm, "");
                                }
                            }, 10000);
                        }
                    });
                } catch (ex) {

                }
                if (anythingChanged) {
                    contentHasChangedSinceValidNess = true;
                    showValidNess();
                }

            }
        }
    };

    function showValidNess() {
        if (!contentHasChangedSinceValidNess) {
            return;
        }
        var almostCompClass = "";
        var incompleteClass = "";
        var fullClass = "";
        if (full) {
            almostCompClass = "hd";
            incompleteClass = "hd";
            fullClass = "i";
            getByQuerySelector("#full").setAttribute("role", "alert");
        }
        else if (valid) {
            incompleteClass = "hd";
            fullClass = "hd";
            almostCompClass = "i";
            getByQuerySelector("#almostcomplete").setAttribute("role", "alert");

        } else {
            almostCompClass = "hd";
            incompleteClass = "w";
            fullClass = "hd";
            getByQuerySelector("#incwn").setAttribute("role", "alert");
        }

        getByQuerySelector("#almostcomplete").className = almostCompClass;
        getByQuerySelector("#full").className = fullClass;
        getByQuerySelector("#incwn").className = incompleteClass;
    };
    function showHelp(elm) {
        try {
            var helpElm = getById(elm.dataset.help);
            helpElm.className = "";
        } catch (e) {
        }
    }

    function hideHelp(elm) {
        try {
            var helpElm = getById(elm.dataset.help);
            helpElm.className = "hd";
        } catch (e) {
        }
    }
  
    [].forEach.call(inputs, function (elm) {
        elm.addEventListener('blur', function (e) {
            lastTouchedField = e.target;
            showValidNess();
            hideHelp(e.target);
        });
        elm.addEventListener('focus', function (e) {
            showHelp(e.target);
        });
    });
    getByQuerySelector("form").addEventListener('change', function (e) {
        setFieldDirty(e);
    });
    getByQuerySelector("form").addEventListener('keyup', function (e) {
        setFieldDirty(e);
    });

   
    setInterval(function () {
        var formStr = getFormStr();
        if (formStr !== lastFormStr) {
            dirty = true;
            lastFormStr = formStr;
        }
    }, 1000);
    setInterval(function () {
        if (dirty && !persistInAction && (Date.now() - lastPress > 250)) {
            postUserFormAjax();
        }
    }, 100);

    function setFieldDirty(e)
    {
        lastPress = Date.now();
        dirty = true;
        var elm = e.target;
        var changedIndicator = getById(elm.id + "_sh");
        changedIndicator.textContent = "";
    }

    return {
        PostUserFormAjax: postUserFormAjax,
        SetPostUrl: function (val) { postUrl = val; }
    }
})();
