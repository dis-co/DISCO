importScripts(
    "flatbuffers.js",
    "../js/Core_generated.js",
    "../js/Api_generated.js",
    "../js/Raft_generated.js",
    "../js/iris.js"
);

onconnect = function (ev) {
    var port = ev.ports[0];
    var context = IrisLib.startWorkerContext();
    context.Register(port);
}
