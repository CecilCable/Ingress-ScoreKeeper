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
            this.getMissingCheckpoints = function (cycleId) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId + "/MissingCheckpoints")
                    .success(function (result) {
                        deferred.resolve(result);
                    });
                return deferred.promise;
            };
            this.getScoreForCp = function (cycleId, checkpoint) {
                var deferred = $q.defer();
                $http.get("/api/" + cycleId + "/" + checkpoint)
                    .success(function (data) {
                        var result = {
                            TimeStamp: data.TimeStamp,
                            Cp: checkpoint,
                            ResistanceScore: data.Cps[0].ResistanceScore,
                            EnlightenedScore: data.Cps[0].EnlightenedScore
                        };
                        deferred.resolve(result);
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
            this.setScore = function (cycleId, timestamp, newScore) {
                var deferred = $q.defer();
                $http.post("/api/" + cycleId + "/" + timestamp, newScore)
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
    .controller("missingCheckpointController", [
        "$scope", "$stateParams", "restService", function ($scope, $stateParams, restService) {
            restService.getMissingCheckpoints($stateParams.cycleId).then(function (missingCheckpoints) {
                $scope.missingCheckpoints = missingCheckpoints;
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
            var timeStamp = score.TimeStamp;
            $scope.submit = function () {
                restService.setScore($stateParams.cycleId, timeStamp, $scope.newScore).then(function () {
                    $state.go("index.overallScore", {cycleId: $stateParams.cycleId});
                });
            };
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
                        "missingCheckpoint": {
                            templateUrl: "/MissingCheckpoint",
                            controller: "missingCheckpointController"

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
                }).state("updateTimeStamp", {
                    url: "/{cycleId:[0-9]*}/{checkpoint:[0-9]*}/{timeStamp:[0-9]*}",
                    controller: "update",
                    templateUrl: "/update",
                    resolve: {
                        score: function ($stateParams) {
                            return {Cp: $stateParams.checkpoint, TimeStamp: $stateParams.timeStamp};
                        }
                    }
                });;

        }
    ]);


angular.element(document).ready(function () {
    angular.bootstrap(document, ["ingressApp"]);
});