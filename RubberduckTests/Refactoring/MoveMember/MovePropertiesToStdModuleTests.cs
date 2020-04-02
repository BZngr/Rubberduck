﻿using NUnit.Framework;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Refactorings.MoveMember;
using System;

namespace RubberduckTests.Refactoring.MoveMember
{
    [TestFixture]
    public class MovePropertiesToStdModuleTests : MoveMemberRefactoringActionTestSupportBase
    {
        private const string ThisStrategy = nameof(MoveMemberToStdModule);

        [TestCase(MoveEndpoints.StdToStd, "Public", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.StdToStd, "Public", "Private Pi As Single")]
        [TestCase(MoveEndpoints.StdToStd, "Private", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.StdToStd, "Private", "Private Pi As Single")]
        [TestCase(MoveEndpoints.ClassToStd, "Public", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.ClassToStd, "Public", "Private Pi As Single")]
        [TestCase(MoveEndpoints.ClassToStd, "Private", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.ClassToStd, "Private", "Private Pi As Single")]
        [TestCase(MoveEndpoints.FormToStd, "Public", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.FormToStd, "Public", "Private Pi As Single")]
        [TestCase(MoveEndpoints.FormToStd, "Private", "Private Const Pi As Single = 3.14")]
        [TestCase(MoveEndpoints.FormToStd, "Private", "Private Pi As Single")]
        [Category("Refactorings")]
        [Category("MoveMember")]
        public void SupportFunctionsReferenceExclusiveSupportNonMember(MoveEndpoints endpoints, string exclusiveFuncAccessibility, string nonMemberDeclaration)
        {
            //Using a global variable to isolate the test to the supporting constant 'Pi'
            var moduleRadius = "RadiusModule";
            var moduleRadiusContent =
@"Option Explicit

Public Radius As Single
";
            var memberToMove = "Area";
            var source =
$@"
Option Explicit

{nonMemberDeclaration}

Public Property Get Area() As Single
    Area = ToArea({moduleRadius}.Radius)
End Property

Public Property Let Area(ByRef areaArg As Single)
    {moduleRadius}.Radius = ToRadius(areaArg) 
End Property

{exclusiveFuncAccessibility} Function ToRadius(ByRef areaArg As Single) As Single
    ToRadius = Sqr(areaArg / Pi)
End Function

{exclusiveFuncAccessibility} Function ToArea(ByRef radius As Single) As Single
    ToArea = Pi * {moduleRadius}.Radius ^ 2
End Function
";

            var destinationExpectedForClassAndFormModules =
$@"
Option Explicit

{nonMemberDeclaration}

Public Property Get Area() As Single
    Area = ToArea({moduleRadius}.Radius)
End Property

Public Property Let Area(ByVal areaArg As Single)
    {moduleRadius}.Radius = ToRadius(areaArg) 
End Property

{exclusiveFuncAccessibility} Function ToRadius(ByRef areaArg As Single) As Single
    ToRadius = Sqr(areaArg / Pi)
End Function

{exclusiveFuncAccessibility} Function ToArea(ByRef radius As Single) As Single
    ToArea = Pi * {moduleRadius}.Radius ^ 2
End Function
";

            var moveDefinition = new TestMoveDefinition(endpoints, (memberToMove, DeclarationType.PropertyGet), sourceContent: source);
            moveDefinition.Add(new ModuleDefinition(moduleRadius, Rubberduck.VBEditor.SafeComWrappers.ComponentType.StandardModule, moduleRadiusContent));

            var refactoredCode = ExecuteTest(moveDefinition);


            StringAssert.AreEqualIgnoringCase(ThisStrategy, refactoredCode.StrategyName);

            if (!moveDefinition.IsStdModuleSource)
            {
                var refactoredLines = refactoredCode.Destination.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in refactoredLines)
                {
                    //Moves everything from Source to Destination as-is
                    Assert.IsTrue(destinationExpectedForClassAndFormModules.Contains(line), $"Failing Content: {line}");
                }
                return;
            }

            //Source Module content checks
            StringAssert.DoesNotContain("Get Area() As Single", refactoredCode.Source);
            StringAssert.DoesNotContain("Let Area(areaArg As Single)", refactoredCode.Source);
            if (exclusiveFuncAccessibility.Equals(Tokens.Private))
            {
                StringAssert.DoesNotContain($"{exclusiveFuncAccessibility} Function ToArea(", refactoredCode.Source);
                StringAssert.DoesNotContain($"{exclusiveFuncAccessibility} Function ToRadius(", refactoredCode.Source);
                StringAssert.DoesNotContain(nonMemberDeclaration, refactoredCode.Source);
            }
            else
            {
                StringAssert.Contains($"{exclusiveFuncAccessibility} Function ToArea(", refactoredCode.Source);
                StringAssert.Contains($"{exclusiveFuncAccessibility} Function ToRadius(", refactoredCode.Source);
                StringAssert.Contains(nonMemberDeclaration, refactoredCode.Source);
            }

            //Destination module content checks
            if (exclusiveFuncAccessibility.Equals(Tokens.Private))
            {
                StringAssert.Contains(nonMemberDeclaration, refactoredCode.Destination);
                StringAssert.Contains($"{exclusiveFuncAccessibility} Function ToArea(", refactoredCode.Destination);
                StringAssert.Contains($"{exclusiveFuncAccessibility} Function ToRadius(", refactoredCode.Destination);
                StringAssert.Contains($"{moduleRadius}.Radius = ToRadius(areaArg)", refactoredCode.Destination);
            }
            else
            {
                StringAssert.DoesNotContain(nonMemberDeclaration, refactoredCode.Destination);
                StringAssert.DoesNotContain($"{exclusiveFuncAccessibility} Function ToArea(", refactoredCode.Destination);
                StringAssert.DoesNotContain($"{exclusiveFuncAccessibility} Function ToRadius(", refactoredCode.Destination);
                StringAssert.Contains($"Area = {moveDefinition.SourceModuleName}.ToArea(", refactoredCode.Destination);
                StringAssert.Contains($"{moduleRadius}.Radius = {moveDefinition.SourceModuleName}.ToRadius(", refactoredCode.Destination);
            }
            StringAssert.Contains("Public Property Get Area(", refactoredCode.Destination);
            StringAssert.Contains("Public Property Let Area(", refactoredCode.Destination);
        }

        [Test]
        [Category("Refactorings")]
        [Category("MoveMember")]
        public void MultiplePropertyGroupsReferenceSameVariable()
        {
            var member = "Foo";
            var source =
$@"
Option Explicit

Private Const mFoo As Long = 10

Public Property Get Foo() As Long
    Foo = mFoo
End Property

Public Property Get FooTimes2() As Long 
    FooTimes2 = mFoo * 2
End Property
";

            var moveDefinition = new TestMoveDefinition(MoveEndpoints.StdToStd, (member, DeclarationType.PropertyGet), sourceContent: source);

            moveDefinition.AddSelectedDeclaration("FooTimes2", DeclarationType.PropertyGet);

            var refactoredCode = ExecuteTest(moveDefinition);

            StringAssert.Contains("Get Foo()", refactoredCode.Destination);
            StringAssert.Contains("Get FooTimes2()", refactoredCode.Destination);
            StringAssert.Contains("Private Const mFoo As Long = 10", refactoredCode.Destination);
        }
    }
}
