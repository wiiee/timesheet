$(function () {
    $.expr[':'].truncated = function (obj) {
        var $this = $(obj);
        var $c = $this
                   .clone()
                   .css({ display: 'inline', width: 'auto', visibility: 'hidden' })
                   .appendTo('body');

        var c_width = $c.width();
        $c.remove();

        if (c_width > $this.width())
            return true;
        else
            return false;
    };
});