using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class MiniInterpreter : MonoBehaviour 
{
    private Dictionary<string, object> Scope = new Dictionary<string, object>();
    private Dictionary<string, Type> UnityTypeMap = new Dictionary<string, Type>();
    public void Exec(string str)
    {
        string[] strlst = str.Split(';');
        for (int i = 0; i <  strlst.Length-1; i++)
        {
            strlst[i] = strlst[i].Replace("\n","") + ";";
            ExecBlock(strlst[i]);
        }

    }
    private object ExecBlock(string code)
    {
        code = code.Trim().TrimEnd(';');
        if (code.Contains('='))
        {
            var parts = code.Split(new[] { '=' }, 2);
            var left = parts[0].Trim();
            var right = parts[1].Trim();
            var leftParts = left.Split(' ');
            if (leftParts.Length == 2)
            {
                var typeName = leftParts[0];
                var varName = leftParts[1];
                var result = InvokeExpression(right);
                Scope[varName] = result;
                return result;
            }
            else
            {
                return AssignProperty(left, right);
            }
        }
        else
        {
            return InvokeExpression(code);
        }
    }
    private object AssignProperty(string left, string right)
    {
        var tokens = left.Split('.');
        if (tokens.Length != 2)
            throw new ArgumentException("Invalid property assignment: " + left);
        var targetName = tokens[0];
        var memberName = tokens[1];
        if (!Scope.ContainsKey(targetName))
            throw new ArgumentException("Unknown variable: " + targetName);
        var target = Scope[targetName];
        var targetType = target.GetType();
        object value = InvokeExpression(right);
        var property = targetType.GetProperty(memberName);
        if (property != null)
        {
            property.SetValue(target, value, null);
            return value;
        }
        var field = targetType.GetField(memberName);
        if (field != null)
        {
            field.SetValue(target, value);
            return value;
        }
        throw new MissingMemberException("No property or field named '" + memberName + "' on type " + targetType.Name);
    }
    private MethodInfo ResolveMethod(Type type, string methodName, BindingFlags flags, List<object> args = null)
    {
        if (args == null)
            args = new List<object>();

        var candidates = type.GetMethods(flags)
            .Where(m => m.Name == methodName && m.GetParameters().Length == args.Count)
            .ToList();

        if (candidates.Count == 0)
            throw new MissingMethodException("No matching method found: "+methodName+" with "+args.Count+" arguments on type "+type.FullName);

        foreach (var candidate in candidates)
        {
            var parameters = candidate.GetParameters();
            bool match = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    if (parameters[i].ParameterType.IsValueType)
                    {
                        match = false;
                        break;
                    }
                }
                else if (!parameters[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return candidate;
        }

        throw new AmbiguousMatchException("Multiple overloads matched for method: "+methodName+" on type "+type.FullName);
    }
    private object InvokeExpression(string expr)
    {
        expr = expr.Trim();
        int it;
        float f;
        bool b;
        if (int.TryParse(expr, out it)) return it;
        if (float.TryParse(expr, out f)) return f;
        if (bool.TryParse(expr, out b)) return b;
        if ((expr.StartsWith("\"") && expr.EndsWith("\"")) || (expr.StartsWith("'") && expr.EndsWith("'")))
            return expr.Substring(1, expr.Length - 2);
        if (Scope.ContainsKey(expr)) return Scope[expr];
        string[] ops = new[] { "+", "-", "*", "/", "%" };
        foreach (var op in ops)
        {
            int index = expr.IndexOf(op);
            if (index > 0)
            {
                string leftPart = expr.Substring(0, index).Trim();
                string rightPart = expr.Substring(index + 1).Trim();
                object left = InvokeExpression(leftPart);
                object right = InvokeExpression(rightPart);

                float lf = EvalToFloat(left);
                float rf = EvalToFloat(right);

                switch (op)
                {
                    case "+": return lf + rf;
                    case "-": return lf - rf;
                    case "*": return lf * rf;
                    case "/": return lf / rf;
                    case "%": return lf % rf;
                }
            }
        }
        var openParen = expr.IndexOf('(');
        var closeParen = expr.LastIndexOf(')');
        if (expr.StartsWith("new "))
        {
            if (openParen == -1 || closeParen == -1 || closeParen <= openParen)
                throw new ArgumentException("Malformed constructor expression: " + expr);
            var typeName = expr.Substring(4, openParen - 4).Trim();
            var ctorArgsPart = expr.Substring(openParen + 1, closeParen - openParen - 1);
            var ctorArgs = ParseArguments(ctorArgsPart);
            var type = ResolveType(typeName);
            var constructors = type.GetConstructors();
            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == ctorArgs.Count)
                {
                    bool match = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (ctorArgs[i] == null || !parameters[i].ParameterType.IsAssignableFrom(ctorArgs[i].GetType()))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return ctor.Invoke(ctorArgs.ToArray());
                }
            }
            throw new MissingMethodException("No matching constructor found for type: " + typeName);
        }
        if (openParen == -1 || closeParen == -1 || closeParen <= openParen)
            throw new ArgumentException("Malformed method expression: " + expr);
        var callPart = expr.Substring(0, openParen);
        var argsPart = expr.Substring(openParen + 1, closeParen - openParen - 1);
        var args = ParseArguments(argsPart);
        string[] callTokens = callPart.Split('.');
        object target = null;
        Type targetType = null;
        MethodInfo method = null;
        if (Scope.ContainsKey(callTokens[0]))
        {
            target = Scope[callTokens[0]];
            targetType = target.GetType();
            string methodName = callTokens[1];
            if (methodName.Contains('<'))
            {
                var baseName = methodName.Substring(0, methodName.IndexOf('<'));
                var genericTypeName = methodName.Substring(methodName.IndexOf('<') + 1);
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('>'));
                var genericType = ResolveType(genericTypeName);

                var rawCandidates = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == baseName && m.IsGenericMethodDefinition)
                    .ToList();

                if (rawCandidates.Count == 0)
                    throw new MissingMethodException("Generic method not found: " + baseName + " on type " + targetType.FullName);

                MethodInfo rawMethod = null;
                foreach (var candidate in rawCandidates)
                {
                    var genericArgs = candidate.GetGenericArguments();
                    if (genericArgs.Length == 1) // Only support single generic arg for now
                    {
                        rawMethod = candidate;
                        break;
                    }
                }

                if (rawMethod == null)
                    throw new AmbiguousMatchException("No suitable generic method found: " + baseName);

                method = rawMethod.MakeGenericMethod(genericType);
                if (rawMethod == null)
                    throw new MissingMethodException("Method not found: " + baseName + " on type " + targetType.FullName);

                method = rawMethod.MakeGenericMethod(genericType);
            }
            else
            {
                method = ResolveMethod(targetType, methodName, BindingFlags.Public | BindingFlags.Instance, args);
            }
        }
        else
        {
            targetType = ResolveType(callTokens[0]);
            string methodName = callTokens[1];
            if (methodName.Contains('<'))
            {
                var baseName = methodName.Substring(0, methodName.IndexOf('<'));
                var genericTypeName = methodName.Substring(methodName.IndexOf('<') + 1);
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('>'));
                var genericType = ResolveType(genericTypeName);
                method = targetType.GetMethod(baseName, BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    throw new MissingMethodException("Method not found: " + methodName + " on type " + targetType.FullName);
                method = method.MakeGenericMethod(genericType);
            }
            else
            {
                var candidates = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == methodName && m.GetParameters().Length == args.Count)
                    .ToList();
                if (candidates.Count == 0)
                    throw new MissingMethodException("No matching method found: " + methodName + " with " + args.Count + " arguments");
                if (candidates.Count > 1)
                {
                    foreach (var candidate in candidates)
                    {
                        var parameters = candidate.GetParameters();
                        bool match = true;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (!parameters[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            method = candidate;
                            break;
                        }
                    }
                    if (method == null)
                        throw new AmbiguousMatchException("Multiple overloads matched for method: " + methodName);
                }
                else
                {
                    method = candidates[0];
                }
            }
        }
        return method.Invoke(target, args.ToArray());
    }
    private float EvalToFloat(object value)
    {
        if (value is int) return (float)(int)value;
        if (value is float) return (float)value;
        throw new ArgumentException("Expected numeric value, got: " + value.GetType());
    }
    private List<object> ParseArguments(string argsPart)
    {
        var args = new List<object>();
        if (string.IsNullOrEmpty(argsPart)) return args;

        var tokens = argsPart.Split(',').Select(s => s.Trim());
        foreach (var token in tokens)
        {
            args.Add(InvokeExpression(token));
        }

        return args;
    }
    private Type ResolveType(string typeName)
    {
        if (UnityTypeMap.ContainsKey(typeName))
            return UnityTypeMap[typeName];
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (type.Name == typeName || type.FullName == typeName || type.FullName.EndsWith("." + typeName))
                {
                    return type;
                }
            }
        }
        throw new TypeLoadException("Type not found: " + typeName);
    }
    public void InitializeInterpreter()
    {
        InjectFields(this);
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine"))
                {
                    UnityTypeMap[type.Name] = type;
                    UnityTypeMap[type.FullName] = type;
                }
            }
        }
    }
    public void InjectFields(object host)
    {
        var fields = host.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            Scope[field.Name] = field.GetValue(host);
        }
    }
}
