﻿using System.Collections.Generic;

namespace RubberduckTests.Refactoring.MoveMember
{
    public struct MoveMemberRefactorResults
    {
        private readonly IDictionary<string, string> _results;
        private readonly string _sourceModuleName;
        private readonly string _destinationModuleName;
        private readonly string _strategyName;

        public MoveMemberRefactorResults(TestMoveDefinition moveDefinition, IDictionary<string, string> refactorResults, string strategy = null)
        {
            _results = refactorResults;
            _sourceModuleName = moveDefinition.SourceModuleName;
            _destinationModuleName = moveDefinition.DestinationModuleName;
            _strategyName = strategy;
        }

        public string this[string moduleName]
        {
            get => _results[moduleName];
        }

        public string Source => _results[_sourceModuleName];
        public string Destination => _results[_destinationModuleName];
        public string StrategyName => _strategyName;
    }
}
