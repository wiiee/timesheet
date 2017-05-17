(function () {
    angular.module("taskTemplate", [])
    .controller("taskTemplateCtrl", function ($scope) {
        $scope.hours = [2, 4, 6, 8, 10, 12, 14, 16];
        $scope.tasks = _tasks ? _tasks : [];
        $scope.oriTasks = angular.copy($scope.tasks);
        $scope.taskIndex = $scope.tasks.length;
        $scope.phases = _phases;

        $scope.addTask = function () {
            $scope.tasks.push({ Id: $scope.taskIndex });

            setTimeout(function () {
                $.validate();
                $(".hours").multiselect({
                    buttonWidth: "100%",
                    onChange: function (option, checked) {
                        option.parent().blur();
                        //$("form").isValid();
                    }
                });

                $scope.taskIndex++;
            }, 50);
        }

        if ($scope.taskIndex === 0) {
            $scope.addTask();
        }
        else {
            for (var i = 0 ; i < $scope.taskIndex; i++) {
                $scope.tasks[i].Id = i;
            }

            setTimeout(function () {
                $.validate();
                $(".hours").multiselect({
                    buttonWidth: "100%",
                    onChange: function (option, checked) {
                        option.parent().blur();
                        //$("form").isValid();
                    }
                });
            }, 50);
        }
        
        $scope.removeTask = function (id) {
            $scope.tasks = _.reject($scope.tasks, {Id: id});
        }

        $scope.reset = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.tasks = angular.copy($scope.oriTasks);

            setTimeout(function () {
                $.validate();
                $(".hours").multiselect({
                    buttonWidth: "100%",
                    onChange: function (option, checked) {
                        option.parent().blur();
                        //$("form").isValid();
                    }
                });
            }, 300);
        };
    });
}());