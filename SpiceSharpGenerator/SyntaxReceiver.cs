﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// A receiver for entities.
    /// </summary>
    public class SyntaxReceiver : ISyntaxReceiver
    {
        private class AttributeComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                // Most common path - both are in their shorted form
                if (x.Length == y.Length)
                    return StringComparer.Ordinal.Equals(x, y);
                if (x.Length > y.Length)
                    return StringComparer.Ordinal.Equals(x, $"{y}Attribute");
                return StringComparer.Ordinal.Equals($"{x}Attribute", y);
            }

            public int GetHashCode(string obj)
            {
                if (obj.EndsWith("Attribute"))
                    return StringComparer.Ordinal.GetHashCode(obj.Substring(0, obj.Length - 9));
                return StringComparer.Ordinal.GetHashCode(obj);
            }
        }
        private Dictionary<string, Action<AttributeSyntax>> _categories = new(new AttributeComparer());

        /// <summary>
        /// Gets the list of eligible entities that may need to be extended using code generation.
        /// </summary>
        /// <value>
        /// The entity classes.
        /// </value>
        public ConcurrentDictionary<ClassDeclarationSyntax, int> Entities { get; } = new ConcurrentDictionary<ClassDeclarationSyntax, int>();

        /// <summary>
        /// Gets a list of behaviors that were tagged with the BehaviorFor attribute.
        /// </summary>
        /// <value>
        /// The behavior classes.
        /// </value>
        public ConcurrentDictionary<ClassDeclarationSyntax, int> Behaviors { get; } = new ConcurrentDictionary<ClassDeclarationSyntax, int>();

        /// <summary>
        /// Gets a list of binding contexts.
        /// </summary>
        /// <value>
        /// The binding context classes.
        /// </value>
        public ConcurrentDictionary<ClassDeclarationSyntax, int> BindingContexts { get; } = new ConcurrentDictionary<ClassDeclarationSyntax, int>();

        /// <summary>
        /// Gets a list of parameter sets.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        public ConcurrentDictionary<ClassDeclarationSyntax, int> ParameterSets { get; } = new ConcurrentDictionary<ClassDeclarationSyntax, int>();

        /// <summary>
        /// Gets a list of parameter sets that need checked properties.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        public ConcurrentDictionary<FieldDeclarationSyntax, int> CheckedFields { get; } = new ConcurrentDictionary<FieldDeclarationSyntax, int>();

        /// <summary>
        /// Creates a new syntax receiver.
        /// </summary>
        public SyntaxReceiver()
        {
            _categories.Add("AutoGeneratedBehaviors", AddGeneratedBehaviors);
            _categories.Add("BehaviorFor", AddBehaviorFor);
            _categories.Add("BindingContextFor", AddBindingContext);
            _categories.Add("GeneratedParameters", AddAutoGeneratedParameters);
            foreach (var name in new[] { 
                "GreaterThan",
                "GreaterThanOrEquals",
                "LessThan",
                "LessThanOrEquals",
                "LowerLimit",
                "UpperLimit",
                "ParameterName",
                "ParameterInfo" })
                _categories.Add(name, AddCheckedField);
        }

        private void AddGeneratedBehaviors(AttributeSyntax attribute)
        {
            // Attributes are part of an attribute list, which are part of a class declaration syntax
            if (attribute.Parent.Parent is ClassDeclarationSyntax cds)
                Entities.AddOrUpdate(cds, 1, (c, i) => ++i);
        }
        private void AddBehaviorFor(AttributeSyntax attribute)
            {
            if (attribute.Parent.Parent is ClassDeclarationSyntax cds)
                Behaviors.AddOrUpdate(cds, 1, (c, i) => ++i);
        }
        private void AddBindingContext(AttributeSyntax attribute)
                {
            if (attribute.Parent.Parent is ClassDeclarationSyntax cds)
                BindingContexts.AddOrUpdate(cds, 1, (c, i) => ++i);
        }
        private void AddAutoGeneratedParameters(AttributeSyntax attribute)
                    {
            if (attribute.Parent.Parent is ClassDeclarationSyntax cds)
                ParameterSets.AddOrUpdate(cds, 1, (c, i) => ++i);
                    }
        private void AddCheckedField(AttributeSyntax attribute)
        {
            if (attribute.Parent.Parent is FieldDeclarationSyntax fds)
                    {
                if (!fds.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword)))
                    CheckedFields.AddOrUpdate(fds, 1, (c, i) => ++i);
            }
                    }

        /// <summary>
        /// Collect whatever we need to create generated code later.
        /// </summary>
        /// <param name="syntaxNode">The syntax node.</param>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
                    {
            if (syntaxNode is not AttributeSyntax attribute)
                        return;
            if (_categories.TryGetValue(attribute.Name.GetText().ToString(), out var action))
                action(attribute);
                }

        private bool IsOneOf(string name, params string[] attributes)
        {
            if (attributes == null || attributes.Length == 0)
                return false;
            for (var i = 0; i < attributes.Length; i++)
            {
                if (string.CompareOrdinal(name, attributes[i]) == 0)
                    return true;
                if (string.CompareOrdinal(name, $"{attributes[i]}Attribute") == 0)
                    return true;
            }
            return false;
        }
    }
}
