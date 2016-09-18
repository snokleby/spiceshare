if (!Date.now) {
    Date.now = function now() {
        return new Date().getTime();
    };
}

var autoSearch = (function () {

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


    var lastFormStr = getFormStr();

    var lastPress = Date.now();
    var dirty = false;
    var persistInAction = false;

    function setsta(inpr) {
        var searchIndicator = document.getElementById("searchpr");
        var dispv = 'none';
        if (inpr) {
            dispv = '';
        }
        if (searchIndicator !== null) {
            searchIndicator.style.display = dispv;
        }
    }

    function getAjax() {
        dirty = false;
        persistInAction = true;
        //give up and reset if no success after 10 sec.
        var failHandle = setTimeout(function () {
            persistInAction = false;
            setsta();
        }, 10000);
        
        DoAutoSearch(function () {
            //actually searching.
            setsta(true);
        },
            function () {
                //done. Called regardless of if sever was hit.
                persistInAction = false;
                clearTimeout(failHandle);
                setTimeout(function () {
                     setsta();
                }, 200);
        });              
    };

    document.querySelector("form").addEventListener('change', function (e) {
        setFieldDirty(e);
    });
    document.querySelector("form").addEventListener('keyup', function (e) {
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
        if (dirty && !persistInAction && (Date.now() - lastPress > 750)) {
            getAjax();
        }
    }, 500);

    function setFieldDirty(e)
    {
        lastPress = Date.now();
        dirty = true;
        var elm = e.target;
        var changedIndicator = document.getElementById(elm.id + "_sh");
        if (changedIndicator !== null) {
            changedIndicator.textContent = "";
        }
    }
    function showHelp(elm) {
        try {
            var helpElm = document.getElementById(elm.dataset.help);
            helpElm.className = "";
        } catch (e) {
        }
    }

    function hideHelp(elm) {
        try {
            var helpElm = document.getElementById(elm.dataset.help);
            helpElm.className = "hd";
        } catch (e) {
        }
    }


    try {
       
        [].forEach.call(inputs, function(elm) {
            elm.addEventListener('blur', function(e) {
                hideHelp(e.target);
            });
            elm.addEventListener('focus', function(e) {
                showHelp(e.target);
            });
        });
    } catch (ex) {
        
    }

    return {
       
    }
})();
