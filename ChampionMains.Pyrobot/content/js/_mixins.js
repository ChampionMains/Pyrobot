Promise.delay = function(duration) {
    return new Promise(function(resolve) {
        setTimeout(resolve, duration);
    });
};
