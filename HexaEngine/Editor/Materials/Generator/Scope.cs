namespace HexaEngine.Editor.Materials.Generator
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IVariableDefinition
    {
    }

    public class Scope
    {
        public Scope(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; set; }

        public List<IVariableDefinition> Variables { get; } = new();
    }

    public class GeneratorContext
    {
        public Stack<Scope> Scopes { get; } = new();

        public string FindFreeVariableName()
        {
            return "v" + Scopes.SelectMany(s => s.Variables).Count();
        }

        public void AddVariableToCurrentScope(IVariableDefinition variable)
        {
            Scopes.Peek().Variables.Add(variable);
        }

        public void EnterNewScope(string scopeIdentifier)
        {
            Scopes.Push(new Scope(scopeIdentifier));
        }

        public void LeaveScope()
        {
            Scopes.Pop();
        }

        public bool IsInScope(IVariableDefinition variable)
        {
            if (variable is null)
            {
                return false;
            }

            return Scopes.Any(s => s.Variables.Contains(variable));
        }
    }
}