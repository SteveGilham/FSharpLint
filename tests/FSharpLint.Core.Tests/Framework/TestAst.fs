﻿module TestAst

open System
open NUnit.Framework
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Configuration
open FSharpLint.Framework.ParseFile
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.SourceCodeServices

let stubBinding attributes =
    SynBinding.Binding(None, 
                       SynBindingKind.NormalBinding, 
                       false, 
                       false, 
                       attributes, 
                       PreXmlDoc.PreXmlDocEmpty, 
                       SynValData(None, SynValInfo.SynValInfo([], SynArgInfo.SynArgInfo([], false, None)), None), 
                       SynPat.Wild(range()), 
                       None, 
                       SynExpr.Null(range()), 
                       range(), 
                       SequencePointInfoForBinding.NoSequencePointAtLetBinding)

let stubConstString str = SynExpr.Const(SynConst.String(str, range()), range())

let stubTuple exprs = SynExpr.Tuple(exprs, [], range())

let stubParen expr = SynExpr.Paren(expr, range(), None, range())

let stubAttribute name argExpr =
    { SynAttribute.AppliesToGetterAndSetter = false
      SynAttribute.Range = range()
      SynAttribute.Target = None
      SynAttribute.TypeName = LongIdentWithDots.LongIdentWithDots([Ident(name, range())], [])
      SynAttribute.ArgExpr = argExpr }

let stubPropertyInitialiser propertyName value =
    SynExpr.App(ExprAtomicFlag.NonAtomic, 
                false, 
                SynExpr.App(ExprAtomicFlag.NonAtomic, false, SynExpr.Ident(Ident("op_Equality", range())), SynExpr.Ident(Ident(propertyName, range())), range()), 
                stubConstString value, 
                range())

[<TestFixture>]
type TestAst() =

    [<Test>]
    member __.GetSuppressMessageAttributesWithConstructorArgs() = 
        let attributes = 
            [ [stubConstString "Analyser"; stubConstString "Rule"] 
              |> stubTuple 
              |> stubParen 
              |> stubAttribute "SuppressMessage" ]

        let binding = AstNode.Binding(stubBinding attributes)

        let attrs = getSuppressMessageAttributes binding

        Assert.AreEqual({ Category = "Analyser"; Rule = "Rule" }, attrs |> List.head |> fst)

    [<Test>]
    member __.GetSuppressMessageAttributesWithPropertyInitialisers() = 
        let attributes = 
            [ [stubPropertyInitialiser "Category" "Analyser"; stubPropertyInitialiser "CheckId" "*"] 
              |> stubTuple 
              |> stubParen 
              |> stubAttribute "SuppressMessage" ]

        let binding = AstNode.Binding(stubBinding attributes)

        let attrs = getSuppressMessageAttributes binding

        Assert.AreEqual({ Category = "Analyser"; Rule = "*" }, attrs |> List.head |> fst)

    [<Test>]
    member __.GetSuppressMessageAttributesWithPropertyInitialisersMissingCategoryProperty() = 
        let attributes = 
            [ [stubPropertyInitialiser "SomeProp" "Analyser"; stubPropertyInitialiser "CheckId" "*"] 
              |> stubTuple 
              |> stubParen 
              |> stubAttribute "SuppressMessage" ]

        let binding = AstNode.Binding(stubBinding attributes)

        Assert.IsEmpty(getSuppressMessageAttributes binding)

    [<Test>]
    member __.GetSuppressMessageAttributesWithPropertyInitialisersMissingCheckIdProperty() = 
        let attributes = 
            [ [stubPropertyInitialiser "Category" "Analyser"; stubPropertyInitialiser "SomeProp" "*"] 
              |> stubTuple 
              |> stubParen 
              |> stubAttribute "SuppressMessage" ]

        let binding = AstNode.Binding(stubBinding attributes)

        Assert.IsEmpty(getSuppressMessageAttributes binding)

    [<Test>]
    member __.GetSuppressMessageAttributesWithPropertyInitialisersWithExtraProperty() = 
        let attributes = 
            [ [ stubPropertyInitialiser "AnotherProp" "gwegweg"
                stubPropertyInitialiser "Category" "Analyser"
                stubPropertyInitialiser "CheckId" "*" ] 
              |> stubTuple 
              |> stubParen 
              |> stubAttribute "SuppressMessage" ]

        let binding = AstNode.Binding(stubBinding attributes)

        let attrs = getSuppressMessageAttributes binding

        Assert.AreEqual({ Category = "Analyser"; Rule = "*" }, attrs |> List.head |> fst)