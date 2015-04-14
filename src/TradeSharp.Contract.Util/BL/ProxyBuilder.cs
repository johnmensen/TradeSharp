using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    public class A : IMockableProxy
    {
        public Dictionary<string, Delegate> MockMethods { get; set; }
        public void IncludeMockMethod(string methodName, Delegate methodImplementation)
        {
            if (MockMethods.ContainsKey(methodName))
                MockMethods[methodName] = methodImplementation;
            else
                MockMethods.Add(methodName, methodImplementation);
        }
    }
    
    public class ProxyBuilder
    {
        private static ProxyBuilder instance;

        public static ProxyBuilder Instance
        {
            get { return instance ?? (new ProxyBuilder()); }
        }
        
        private readonly Dictionary<Type, Type> implementers = new Dictionary<Type, Type>();

        private ProxyBuilder()
        {
            var typesToImpl = new []
                {
                    new Cortege2<Type, bool>(typeof (ITradeSharpAccount), true),
                    new Cortege2<Type, bool>(typeof (ITradeSharpDictionary), false),
                    new Cortege2<Type, bool>(typeof (ITradeSharpServerTrade), true),
                    new Cortege2<Type, bool>(typeof (IWalletManager), false),
                    new Cortege2<Type, bool>(typeof (ITradeSharpServer), false),
                    new Cortege2<Type, bool>(typeof (IAccountStatistics), true),
                    new Cortege2<Type, bool>(typeof (ITradeSignalExecutor), false),
                    new Cortege2<Type, bool>(typeof (IPlatformManager), false), 
                };
            
            var asm = MakeAssembly(typesToImpl);
            foreach (var tp in asm.GetTypes())
            {
                implementers.Add(tp.GetInterfaces()[0], tp);
            }
        }

        public T GetImplementer<T>() where T : class
        {
            var tpInfo = implementers[typeof(T)];
            var constructorInfo = tpInfo.GetConstructor(new Type[0]);
            if (constructorInfo != null) return (T)constructorInfo.Invoke(new object[0]);
            return null;
        }

        public T MakeImplementer<T>(bool rethrowException) where T : class
        {
            var asm = MakeAssembly(new [] { new Cortege2<Type, bool>(typeof(T), rethrowException) });
            var implType = asm.GetTypes()[0];
            return (T) Activator.CreateInstance(implType);
        }

        public static string GetMethodName<T>(Expression<Action<T>> method)
        {
            return ((MethodCallExpression)method.Body).Method.Name;
        }

        private Assembly MakeAssembly(Cortege2<Type, bool>[] typesToImpl)
        {
            var srcCode = new StringBuilder();

            UsingNameSpaseImplement(srcCode);

            srcCode.AppendLine("namespace TradeSharp.Contract.Proxy");
            srcCode.AppendLine("{");
            foreach (var tp in typesToImpl)
                MakeImplementer(srcCode, tp.a, tp.b, true);
            srcCode.AppendLine("}");

            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters { GenerateInMemory = true, GenerateExecutable = false };

            parameters.ReferencedAssemblies.Add("system.dll");
            parameters.ReferencedAssemblies.Add("mscorlib.dll");
            parameters.ReferencedAssemblies.Add("System.ServiceModel.dll");

            var nameTradeSharpContract = GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(x => x.Name == "TradeSharp.Contract");
            if (nameTradeSharpContract != null)
                parameters.ReferencedAssemblies.Add(Assembly.ReflectionOnlyLoad(nameTradeSharpContract.FullName).Location);

            var nameTradeSharpUtil = GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(x => x.Name == "TradeSharp.Util");
            if (nameTradeSharpUtil != null)
                parameters.ReferencedAssemblies.Add(Assembly.ReflectionOnlyLoad(nameTradeSharpUtil.FullName).Location);

            parameters.ReferencedAssemblies.Add(GetType().Assembly.Location);
            var results = codeProvider.CompileAssemblyFromSource(parameters, srcCode.ToString());
            return !results.Errors.HasErrors ? results.CompiledAssembly : null;
        }

        private void MakeImplementer(StringBuilder sb, Type typeToImpl, bool rethrowEx, bool isMockProxy = false)
        {
            var typeName = typeToImpl.Name.Substring(1);
            sb.AppendLine("     public class " + typeName + "Proxy : " + typeToImpl.Name + 
                (isMockProxy ? ", IDisposable, IMockableProxy" : ""));
            sb.AppendLine("     {");

            sb.AppendLine("         private ChannelFactory<" + typeToImpl.Name + "> factory;");
            sb.AppendLine("         private " + typeToImpl.Name + " channel;");
            sb.AppendLine("         private readonly string endpointName;");
            sb.AppendLine("         public object dataObject = null;");

            if (isMockProxy)
            {
                sb.AppendLine("         private Dictionary<string, Delegate> mockMethods = new Dictionary<string, Delegate>();");
                sb.AppendLine("         public Dictionary<string, Delegate> MockMethods { get { return mockMethods; } set { mockMethods = value; } }");

                sb.AppendLine("         public void IncludeMockMethod(string methodName, Delegate methodImplementation)");
                sb.AppendLine("         {");
                sb.AppendLine("             if (mockMethods.ContainsKey(methodName))");
                sb.AppendLine("                 mockMethods[methodName] = methodImplementation;");
                sb.AppendLine("             else");
                sb.AppendLine("                 mockMethods.Add(methodName, methodImplementation);");
                sb.AppendLine("         }");
            }

            sb.AppendLine("");
            sb.AppendLine("         public " + typeName + "Proxy()");
            sb.AppendLine("         {");
            sb.AppendLine("             this.endpointName = \"I" + typeName + "Binding\";");
            sb.AppendLine(isMockProxy ? "            " : "            RenewFactory();");
            sb.AppendLine("         }");

            sb.AppendLine("");
            sb.AppendLine("         private void RenewFactory()");
            sb.AppendLine("         {");
            sb.AppendLine("             try");
            sb.AppendLine("             {");
            sb.AppendLine("                 if (factory != null) factory.Abort();");
            sb.AppendLine("                 factory = new ChannelFactory<" + typeToImpl.Name + ">(endpointName);");
            sb.AppendLine("                 channel = factory.CreateChannel();");
            sb.AppendLine("             }");
            sb.AppendLine("             catch (Exception ex)");
            sb.AppendLine("             {");
            sb.AppendLine("                 Logger.Error(\"" + typeName + "Proxy: невозможно создать прокси\", ex);");
            sb.AppendLine("                 channel = null;");
            sb.AppendLine("             }");
            sb.AppendLine("         }");

            sb.AppendLine("");
            sb.AppendLine("         public void Dispose()");
            sb.AppendLine("         {");
            sb.AppendLine("             factory.Close();");
            sb.AppendLine("         }");

            sb.AppendLine("");
            foreach (var method in typeToImpl.GetMethods())
            {
                MethodImplement(sb, rethrowEx, method, isMockProxy);
            }
            sb.AppendLine("     }");
        }

        private static void UsingNameSpaseImplement(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ServiceModel;");
            sb.AppendLine("using TradeSharp.Contract.Contract;");
            sb.AppendLine("using TradeSharp.Contract.Util.BL;");
            sb.AppendLine("using TradeSharp.Contract.Entity;");
            sb.AppendLine("using TradeSharp.Util;");
            sb.AppendLine("");
        }

        /// <summary>
        /// Реализация метода с сигнатурой без выходных параметров
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="rethrowEx"></param>
        /// <param name="method"></param>
        private static void MethodImplement(StringBuilder sb, bool rethrowEx, MethodInfo method, bool isMockProxy)
        {
            const string tab = "         ";
            var signature = method.GetSignature();
            var outRefParams = GetOutRefParams(signature);
            var paramNames = method.GetParameters().Select(x => x.Name).ToList();
            var paramsWithModif = method.GetParameters().Select(x => x.Name).ToList();
            for (var i = 0; i < paramNames.Count; i++)
            {
                var outrefParam = outRefParams.Where(x => x.c == paramNames[i]).ToList();
                if (outrefParam.Any())
                {
                    paramsWithModif[i] = outrefParam.First().a + " " + paramNames[i];
                }
            }
            var callSignature = method.Name + "(" + string.Join(",", paramsWithModif) + ");";
            var returnStr = tab + "     return channel." + callSignature;
            if (method.ReturnType.Name.ToLower() == "void")
                returnStr = tab + "     channel." + callSignature;
            var retutnType = MethodInfoExtensions.TypeName(method.ReturnType);



            sb.AppendLine();
            sb.AppendLine(tab + signature);
            sb.AppendLine(tab + "{");

            if (isMockProxy)
            {
                #region isMockProxy
                sb.AppendLine(tab + "     if (mockMethods.ContainsKey(" + "\"" + method.Name + "\"" + "))");
                sb.AppendLine(tab + "     {");
                sb.AppendLine(tab + "         var del = mockMethods[" + "\"" + method.Name + "\"" + "];");
                foreach (var outRefParam in outRefParams)
                {
                    sb.AppendLine(tab + "         " + outRefParam.c + " = " + "default(" + outRefParam.b + ");");
                }
                sb.AppendLine(tab + "         var ptrs = new object[] {" + string.Join(",", paramNames) + "};");
                sb.AppendLine(tab + "         var retVal = del.DynamicInvoke(ptrs);");
                for (var i = 0; i < paramNames.Count; i++)
                {
                    var outParam = outRefParams.Select(x => x).FirstOrDefault(x => x.c == paramNames[i]);
                    if (outParam.c != null)
                    {
                        sb.AppendLine(tab + "         " + outParam.c + " = " + "(" + outParam.b + ")ptrs[" + i + "];");
                    }
                }
                if (retutnType.ToLower() != "void")
                {
                    sb.AppendLine(tab + "         return (" + MethodInfoExtensions.TypeName(method.ReturnType) + ")retVal;");
                }
                else
                {
                    sb.AppendLine(tab + "         return;");
                }
                sb.AppendLine(tab + "     }");
                #endregion
                
            }



            sb.AppendLine();
            sb.AppendLine(tab + "     try");
            sb.AppendLine(tab + "     {");
            sb.AppendLine(tab + returnStr);
            sb.AppendLine(tab + "     }");
            sb.AppendLine(tab + "     catch (Exception)");
            sb.AppendLine(tab + "     {");
            sb.AppendLine(tab + "         RenewFactory();");
            sb.AppendLine(tab + "         try");
            sb.AppendLine(tab + "         {");
            sb.AppendLine(tab + "    " + returnStr);
            sb.AppendLine(tab + "         }");
            sb.AppendLine(tab + "         catch (Exception ex)");
            sb.AppendLine(tab + "         {");
            sb.AppendLine(tab + "             Logger.Error(\"" + method.Name + "()\", ex);");

            if (rethrowEx)
            {
                sb.AppendLine(tab + "         throw;");
            }
            else
            {
                foreach (var outRefParam in outRefParams)
                {
                    sb.AppendLine(tab + "         " + outRefParam.c + " = " + "default(" + outRefParam.b + ");");
                }
                if (method.ReturnType.Name.ToLower() != "void")
                {
                    if (method.ReturnType.IsValueType)
                        sb.AppendLine(tab + "        return " + "default(" + method.ReturnType.Name + ");");
                    else
                        sb.AppendLine(tab + "        return null;");
                }
            }

            sb.AppendLine(tab + "         }");
            sb.AppendLine(tab + "     }");
            sb.AppendLine(tab + "}");
        }

        /// <summary>
        /// Вытаскивает из строковой сигнатуры параметры с модификаторами (в виде списка)
        /// </summary>
        /// <param name="signature">сигнатура метода в виде строки</param>
        /// <returns>список из кортежей "модификатор" - "тип" - "имя". Например, out - int - accountId</returns>
        private static List<Cortege3<string, string, string>> GetOutRefParams(string signature)
        {
            var result = new List<Cortege3<string, string, string>>();
            var signatureParam =
                signature.Substring(signature.IndexOf('(') + 1, signature.IndexOf(')') - signature.IndexOf('(') - 1).Split(',');
            foreach (var param in signatureParam)
            {
                var items = param.Trim().Split(' ');
                if (items.Length == 3)
                    result.Add(new Cortege3<string, string, string>(items[0].Trim(), items[1].Trim(), items[2].Trim()));
            }

            return result;
        }
    }
}
