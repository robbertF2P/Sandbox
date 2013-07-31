/// <reference path="_references.js" />
if (!nSignalR) var nSignalR = {};

$(function () {

   nSignalR.SignalRStarter = {
        HubsRegistered: false,
        Callbacks: [],

        registerOnStart: function (startCallback) {
           nSignalR.SignalRStarter.HubsRegistered = true;

           nSignalR.SignalRStarter.Callbacks.push(startCallback);
        },

        start: function () {
            if (nSignalR.SignalRStarter.HubsRegistered) {
                $.connection.hub.start(function () {
                    $.each(nSignalR.SignalRStarter.Callbacks, function (index, callback) {
                        callback();
                    });
                });
            }
        }
    };

   nSignalR.EventHub = {
        MessageElement: null,
        LogonLocation: null,

        init: function (sessionId, messageElement) {

           nSignalR.EventHub.MessageElement = messageElement;
           
            var eventHub = $.connection.eventHub;

            eventHub.client.displayMessage = function (message) {
                $.pnotify({
                    title: message,
                    text: 'I received a event named: ' + message,
                    icon: 'icon-envelope'
                });
            };

           nSignalR.SignalRStarter.registerOnStart(function () {
                eventHub.server.init(sessionId);
            });
        }
    };

});