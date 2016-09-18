(function () {
 
    setTimeout(function () {
        var request = new XMLHttpRequest();
        request.open("GET", "/api/prune?v=" + Math.random());
        request.send(null);
        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                if (request.status === 200) {
                    var elm = document.getElementById("resetinfo");
                    elm.innerHTML = request.responseText;
                    var s = document.createElement('script');
                    s.type = 'text/javascript';
                    s.async = true;
                    s.src = '/jsmin/lazy.min.js';
                    var x = document.getElementsByTagName('script')[0];
                    x.parentNode.insertBefore(s, x);
                }
            }
        };       
    }, 1);  
})();
jsfl = true;