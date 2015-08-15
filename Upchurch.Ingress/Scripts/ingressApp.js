"use strict";
angular.module("ingressApp", ["ui.router", "ui.bootstrap"])
    .controller("findCurrentCycle", [
        "restService", "$state", function (restService, $state) {
            restService.getCurrentCycle().then(
                function (cycleId) {
                    $state.go("index.overallScore", {cycleId: cycleId});
                }
            );
        }
    ])
    .service("restService", [
        "$http", "$q",
        function ($http, $q) {
            this.getCurrentCycle = function () {
                var deferred = $q.defer();
                $http.get("/api")
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
            this.getOverallScore = function (cycleId) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId + "/OverallScore")
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
            this.getSummary = function (cycleId) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId + "/Summary")
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
            this.getScoreForCp = function (cycleId, checkpoint) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId + "/" + checkpoint)
                    .success(function (data) {
                        if (data) {
                            deferred.resolve(data);
                        } else {
                            deferred.resolve({});
                        }

                    });
                return deferred.promise;
            };
            this.getScores = function (cycleId) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId)
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
            //$stateParams.cycleId, 35, overallScore.TimeStamp, $scope.calculatedResistanceScore(), $scope.calculatedResistanceScore
            this.setScore = function (cycleId, checkpoint, updatedScore) {
                var deferred = $q.defer();
                $http.post("/api/" + cycleId + "/" + checkpoint, updatedScore)
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
        }
    ])
    .controller("overallController", [
        "$scope", "$stateParams", "restService", function ($scope, $stateParams, restService) {

            restService.getOverallScore($stateParams.cycleId).then(function (overallScore) {
                $scope.overallScore = overallScore;
            });
        }
    ])
    .controller("scoresController", [
        "$scope", "$stateParams", "restService", function ($scope, $stateParams, restService) {

            restService.getScores($stateParams.cycleId).then(function (scores) {
                $scope.scores = scores;
            });
        }
    ])
    .controller("summaryController", [
        "$scope", "$stateParams", "restService", function ($scope, $stateParams, restService) {
            restService.getSummary($stateParams.cycleId).then(function (summary) {
                $scope.summary = summary;
            });
        }
    ])
    .controller("update", [
        "$scope", "$stateParams", "$state", "restService", "score", function ($scope, $stateParams, $state, restService, score) {
            $scope.newScore = score;
            $scope.submit = function () {
                //newScore will be something like {"EnlightenedScore":156537,"ResistanceScore":179583,"TimeStamp":"635751832625510213","Kudos":"Foo"}
                if (!$scope.newScore.EnlightenedScore) {
                    alert("EnlightenedScore is not valid");
                    return;
                }
                if (!$scope.newScore.ResistanceScore) {
                    alert("ResistanceScore is not valid");
                    return;
                }
                var enlightenedScore = $scope.newScore.EnlightenedScore;
                if (enlightenedScore.replace) {
                    enlightenedScore = Number(enlightenedScore.replace(",", ""));
                    if (!enlightenedScore) {
                        alert("EnlightenedScore is not valid");
                        return;
                    }
                }
                var registanceScore = $scope.newScore.ResistanceScore;
                if (registanceScore.replace) {
                    registanceScore = Number(registanceScore.replace(",", ""));
                    if (!registanceScore) {
                        alert("ResistanceScore is not valid");
                        return;
                    }
                }
                restService.setScore($stateParams.cycleId, $stateParams.checkpoint, {
                    EnlightenedScore:enlightenedScore,
                    ResistanceScore:registanceScore,
                    TimeStamp: score.TimeStamp,
                    Kudos: $scope.newScore.Kudos
                }).then(function () {
                    $state.go("index.overallScore", {cycleId: $stateParams.cycleId});
                });
            };
        }
    ])
    .controller("update35", [
        "$scope", "$stateParams", "$state", "restService", "score", "overallScore", function ($scope, $stateParams, $state, restService, score, overallScore) {
            $scope.newScore = score;
            $scope.EnlightenedScore = score.EnlightenedScore;//save this off so we can back out the score correctly
            $scope.ResistanceScore = score.ResistanceScore;//save this off so we can back out the score correctly
            $scope.overallScore = overallScore;
            $scope.EnlightenedCycleScore = overallScore.EnlightenedScore;
            $scope.ResistanceCycleScore = overallScore.ResistanceScore;
            $scope.submit = function () {
                restService.setScore($stateParams.cycleId, 35, $scope.newScore)
                    .then(function () {
                        $state.go("index.overallScore", { cycleId: $stateParams.cycleId });
                    });
            };
            var calculateScore = function (totalScoreSoFar, typedInFinalScore, cp35Score) {
                if (!typedInFinalScore) {
                    return "type in cycle score";
                }

                if (typedInFinalScore.replace) {
                    typedInFinalScore = Number(typedInFinalScore.replace(",", ""));
                    if (!typedInFinalScore) {
                        alert("input is not valid");
                    }
                }


                //no 35 CP recorded
                if (!cp35Score) {
                    cp35Score = 0;
                }
                else if (Object.prototype.toString.call(cp35Score) == '[object String]') {
                    cp35Score = 0;
                }

                var scoreMinimum = (totalScoreSoFar - cp35Score) / 35;
                scoreMinimum = Math.ceil(scoreMinimum / 1000) * 1000;//since final scores are given in kilobytes

                var newCp35Score = typedInFinalScore * 35 - totalScoreSoFar + cp35Score;
                
                
                if (typedInFinalScore < scoreMinimum) {
                    return "Cycle score must be at least " + scoreMinimum;
                }
                return newCp35Score;
            }
            $scope.$watch("EnlightenedCycleScore", function (newValue) {
                if (!newValue) {
                    return;//don't delete 
                }
                $scope.newScore.EnlightenedScore = calculateScore($scope.overallScore.EnlightenedScoreTotal, newValue, $scope.EnlightenedScore);
            });
            $scope.$watch("ResistanceCycleScore", function (newValue) {
                if (!newValue) {
                    return;//don't delete 
                }
                $scope.newScore.ResistanceScore = calculateScore($scope.overallScore.ResistanceScoreTotal, newValue, $scope.ResistanceScore);
            });

           
        }
        ])
    .run(
        [
            "$rootScope", "$state", "$stateParams",
            function ($rootScope, $state, $stateParams) {

                // It's very handy to add references to $state and $stateParams to the $rootScope
                // so that you can access them from any scope within your applications.For example,
                // <li ng-class="{ active: $state.includes('contacts.list') }"> will set the <li>
                // to active whenever 'contacts.list' or one of its decendents is active.
                $rootScope.$state = $state;
                $rootScope.$stateParams = $stateParams;
            }
        ]
    )
    .config(
    [
        "$stateProvider", "$urlRouterProvider",
        function ($stateProvider, $urlRouterProvider) {
            $urlRouterProvider

                // If the url is ever invalid, e.g. '/asdf', then redirect to '/' aka the home state
                .otherwise("/");

            $stateProvider
                .state("findCycle", {
                    url: "/",
                    controller: "findCurrentCycle"
                })
                .state("index", {
                    templateUrl: "/angular",
                    abstract: true
                })
                .state("index.overallScore", {
                    url: "/{cycleId:[0-9]*}",
                    views: {
                        "overallScore": {
                            templateUrl: "/OverallScore",
                            controller: "overallController"
                        },
                        "scores": {
                            templateUrl: "/Scores",
                            controller: "scoresController"

                        },
                        "summary": {
                            templateUrl: "/Summary",
                            controller: "summaryController"
                        }
                    }
                })
                .state("update35", {
                    url: "/{cycleId:[0-9]*}/35",
                    controller: "update35",
                    templateUrl: "/update35",
                    resolve: {
                        score: function ($stateParams, restService, $q) {
                            var promise = $q.defer();
                            restService.getScoreForCp($stateParams.cycleId, 35).then(function (score) {
                                promise.resolve(score);
                            });
                            return promise.promise;
                        },
                        overallScore: function (restService, $stateParams) {
                            return restService.getOverallScore($stateParams.cycleId);
                        }
                    }
                })
                .state("update", {
                    url: "/{cycleId:[0-9]*}/{checkpoint:[0-9]*}",
                    controller: "update",
                    templateUrl: "/update",
                    resolve: {
                        score: function ($stateParams, restService, $q) {
                            var promise = $q.defer();
                            restService.getScoreForCp($stateParams.cycleId, $stateParams.checkpoint).then(function (score) {
                                promise.resolve(score);
                            });
                            return promise.promise;
                        }
                    }
                });
                //.state("updateTimeStamp", {
                //    url: "/{cycleId:[0-9]*}/{checkpoint:[0-9]*}/{timeStamp:[0-9]*}",
                //    controller: "update",
                //    templateUrl: "/update",
                //    resolve: {
                //        score: function ($stateParams) {
                //            return {Cp: $stateParams.checkpoint, TimeStamp: $stateParams.timeStamp};
                //        }
                //    }
                //});;

        }
    ]);


angular.element(document).ready(function () {
    angular.bootstrap(document, ["ingressApp"]);
});