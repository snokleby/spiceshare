# spiceshare

My goal with this 10k Apart project is to show that it is possible to make a fully functional, modern, web-application 
within the limits of the competition. In modern browsers if features auto-save, geo-location, in-browser image capture etc.
Since it is built on web-standards, it will work in every non-modern browser as well, falling back to standard file upload, form posting etc.

Images are processed by Microsoft Cognitive Services. 
This filters bad content and provides a default alt-text to the image.

## Performance

The CSS is hand-written to make it as small as possible, and a minified version is inlined in the HTML.
Javascript is loaded async, and for the most common navigation-paths prefetch hints are inserted into the DOM after load.

The logo is in SVG, stripped for meta-data and inlined. 

Images are resized and stripped for meta-data after they are uploaded by the user. 
The default src for the image is resized to always be less than 2.4 kB, to ensure the whole page
is loaded in less than 10kB.

## Interoperability and Progressive enhancement 

A tiny bit of Javascript is inlined at the start of the HTML. This code generates a css-class that is used to 
only show content when JS is not loaded. It also sets of a timer, that will remove this class if the async main javascript
is not loaded (and parsed without error) within a timeout. 
This ensures that the page is usable in conditions where the browser supports JS, but JS did not load correctly.

If javascript does not execute as expected, the site can still be used. Ajax is replaced with standard page-load.
Some features like detection of your current position will not work without Javascript, but you can enter your
position in regular input-fields.

If the browser supports it, photos can be taken directly from the browser, via an in-page video-frame.
Images can also be uploaded via the normal file-upload feature, that all browsers support.
On most phones this will trigger the camera as well.

If the browser does not support SVG, a fallback-text will be displayed.

## Accessibility

Care has been taken to make the site as accessible as possible. It can be keyboard-navigated, 
the contrast is within WCAG 2.0 standards, and the form-fields have aira-attributes where needed.
When something if loaded with ajax, live-regions are used to alert the user of this.
The users are prompted to add alt-text to the images they upload, and a default text will be auto-generated
from the content of the image, using Microsoft Cognitive Services.

