$(function () {
    $("body").on("click", "a.return", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to return this timesheet?", function (result) {
            if (result) {
                location = _basePath + "/TimeSheet/ReturnTimeSheet?monday=" + element.attr("data-monday") + "&userId=" + element.attr("data-userId");
            }
        });
    });

    var users = new Vue({
        el: '#users',
        created: function () {
            var url = _basePath + "/api/TimeSheet/GetManageTimeSheetModel";
            this.$http.post(url).then(response => {
                this.showUsers[0] = response.data;

                this.showUsers[1] = {};
                var data = response.data;
                for (var userId in data) {
                    this.$set(this.text, userId, this.showTexts[1]);
                    this.text[userId] = this.showTexts[1];
                    this.showUsers[1][userId] = [];
                    for (var rowId in data[userId]) {
                        if (data[userId][rowId].Status !== "Done") {
                            var item = {};
                            $.extend(item, data[userId][rowId])
                            this.showUsers[1][userId].push(item);
                        }
                    }
                }

                for (var userId in this.showUsers[1]) {
                    this.$set(this.users, userId, this.showUsers[1][userId]);
                }
            }, response => { });

            var names = this.names;
            url = _basePath + "/api/user/GetUserNames";
            this.$http.post(url).then(response => {
                this.names = response.data;
            }, response => { });
        },
        data: {
            showUsers: {
                0: {},
                1: {}
            },
            users: {},
            names: {},
            showTexts: {
                0: "Show All",
                1: "Show Recently"
            },
            text: {}
        },
        methods: {
            switchShowAll: function (userId) {
                if (this.text[userId] === this.showTexts[0]) {
                    //this.$set(this.text, userId, this.showTexts[1]);
                    //this.$set(this.users, userId, this.showUsers[1][userId]);

                    this.text[userId] = this.showTexts[1];
                    this.users[userId] = this.showUsers[1][userId];
                }
                else {
                    //this.$set(this.text, userId, this.showTexts[1]);
                    //this.$set(this.users, userId, this.showUsers[1][userId]);

                    this.text[userId] = this.showTexts[0];
                    this.users[userId] = this.showUsers[0][userId];
                }
                //this.$set(this.currentTextIndex, userId, index);
                //this.currentTextIndex = Object.assign({}, this.currentTextIndex, { userId: index });
                //this.$set(this.showText, userId, this.showTexts[index]);
                //this.showText = Object.assign({}, this.showText, { userId: this.showTexts[index] });
            },
            getTimeSheetId: function (monday) {
                return utility.getTimeSheetId(monday);
            },
            editOrRead: function (status, monday, userId) {
                return "TimeSheet/" + status === "Done" ? "Read" : "Edit" + "TimeSheet?monday=" + this.getTimeSheetId(monday) + "&userId=" + userId;
            }
        }
    })
});