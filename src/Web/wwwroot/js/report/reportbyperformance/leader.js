$(function () {
    var users = new Vue({
        el: '#performanceItems',
        created: function () {
            var url = _basePath + "/api/TimeSheet/GetManageTimeSheetModel";
            this.$http.post(url).then(function(response) {
                this.showUsers[0] = response.data;

                this.showUsers[1] = {};
                var data = response.data;
                for (var userId in data) {
                    this.$set(this.text, userId, this.showTexts[0]);
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
            }, function(error) { });

            var names = this.names;
            url = _basePath + "/api/user/GetUserNames";
            this.$http.post(url).then(function(response) {
                this.names = response.data;
            }, function(error) { });
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
                    this.text[userId] = this.showTexts[1];
                    this.users[userId] = this.showUsers[0][userId];
                }
                else {
                    this.text[userId] = this.showTexts[0];
                    this.users[userId] = this.showUsers[1][userId];
                }
            },
            getTimeSheetId: function (monday) {
                return utility.getTimeSheetId(monday);
            },
            editOrRead: function (status, monday, userId) {
                return (status === "Done" ? "Read" : "Edit") + "TimeSheet?monday=" + this.getTimeSheetId(monday) + "&userId=" + userId;
            }
        }
    })
});