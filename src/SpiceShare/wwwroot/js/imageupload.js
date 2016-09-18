(function () {

    function getById(id) {
        return document.getElementById(id);
    }
   

    var canvas = getById('canvas');
    var video = getById('video');

    function html5videofailed() {
        getById('enablefbct').className = 'hd';
        getById('fbct').className = '';
        video.style.display = "none";
    }

    var context;
    try {
        context = canvas.getContext('2d');
    } catch (ex) {
        html5videofailed();
        return;
    }
  
    var videoSelect = getById('videoSource');
  
    function gotSources(sourceInfos) {
       
        for (var i = 0; i !== sourceInfos.length; ++i) {
            var sourceInfo = sourceInfos[i];
            var option = document.createElement('option');
            option.value = sourceInfo.deviceId;
            if (option.value === undefined) {
                option.value = sourceInfo.id;
            }
            if (sourceInfo.kind === 'video' || sourceInfo.kind === 'videoinput') {
                option.text = sourceInfo.label || 'camera ' + (videoSelect.length + 1);
                videoSelect.appendChild(option);
            } 
        }

        if (sourceInfos.length > 0) {
            getById('html5videoarea').className = '';
        } else {
            html5videofailed();
        }
    }


    try {        
        navigator.mediaDevices.enumerateDevices()
            .then(gotSources)
            ["catch"](html5videofailed);
    } catch (ex) {
        try {      
            MediaStreamTrack.getSources(gotSources);    
        } catch (e) {
            html5videofailed();
        }
    }

    function camFailed() {
        var camfail = getById("camfail");
        camfail.textContent = "Something went wrong. Try taking a picture using the button below instead.";
        camfail.style.display = "";
    }

    function StartVideoCapture() {
        getById('cameraeara').className = '';
             
        var videoSource = videoSelect.value;
        var mediaConfig = {
            video: {
                optional: [
                    {
                        sourceId: videoSource
                    }
                ]
            }
        };
        // Put video listeners into place
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            var p = navigator.mediaDevices.getUserMedia(mediaConfig);
            p.then(function (stream) {
                video.src = window.URL.createObjectURL(stream);
                video.play();
                setVideoCaptureOk();
            });
           
            p["catch"](function() {
                camFailed();
                html5videofailed();
            });
           
        }
        /* Legacy code below! */
        else if (navigator.getUserMedia) { // Standard
            navigator.getUserMedia(mediaConfig, function(stream) {
                video.src = stream;
                video.play();
                setVideoCaptureOk();
            }, function () {
                html5videofailed();
                camFailed();
            });
        } else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
            navigator.webkitGetUserMedia(mediaConfig, function(stream) {
                video.src = window.webkitURL.createObjectURL(stream);
                video.play();
                setVideoCaptureOk();
            }, function () {
                html5videofailed();
                camFailed();
            });
        } else if (navigator.mozGetUserMedia) { // Mozilla-prefixed
            navigator.mozGetUserMedia(mediaConfig, function(stream) {
                video.src = window.URL.createObjectURL(stream);
                video.play();
                setVideoCaptureOk();
            }, function () {
                html5videofailed();
                camFailed();
            });
        } else {
            html5videofailed();
            camFailed();
        }

    }

    function setVideoCaptureOk() {
        setTimeout(function () {
            getById('snapwait').style.display = 'none';
            getById('cambtnsec').style.display = '';
        }, 1000);
    }

    getById('startcapture').addEventListener('click', function(e) {
        StartVideoCapture();
        e.preventDefault();
    });

    getById("enh5ct").addEventListener('click', function(e) {
        getById("h5ct").className = '';
        e.preventDefault();
    });
    getById("enablefbct").addEventListener('click', function (e) {
        getById("fbct").className = '';
        e.preventDefault();
    });
    getById("enh5ct").style.display = '';

    getById("enablefbct").style.display = '';

// Trigger photo take
    getById('snap').addEventListener('click', function (e) {
        var videoHeight = video.videoHeight;
        var videoWidth = video.videoWidth;
        canvas.width = videoWidth;
        canvas.height = videoHeight;

        context.drawImage(video, 0, 0, videoWidth, videoHeight);
        video.style.display = 'none';
        canvas.style.display = '';
        var progress = getById('waitupload');
        progress.style.display = '';
        progress.textContent = "Wait while the picture is uploaded and analyzed.";
        var dotTimer = setInterval(function() {
            progress.textContent = progress.textContent + ".";
        },350);
        var dataURL = canvas.toDataURL('image/jpeg', 0.5);
        var request = new XMLHttpRequest();
        request.open('POST', this.dataset.uploadpath, true);
        request.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');
        request.send(dataURL);
        request.onreadystatechange = function() {
            if (request.readyState === 4) {
                clearInterval(dotTimer);
                var failElm = getById('uploadfail');
                if (request.status === 200) {
                    getById('uploaddone').style.display = '';
                    failElm.style.display = "none";
                    getById('waitupload').style.display = 'none';
                    document.location.href = getById('snap').dataset.returnpath;
                } else {
                    failElm.style.display = '';
                    failElm.textContent = request.responseText;
                    getById('waitupload').style.display = 'none';
                }
            }
        };

    });

})();