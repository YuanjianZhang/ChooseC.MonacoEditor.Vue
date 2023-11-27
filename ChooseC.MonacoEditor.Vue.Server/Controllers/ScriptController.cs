using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.Serialization;
using ChooseC.MonacoEditor.Api.Models;
using CSScriptLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Composition.Hosting;
using System.Reflection;

namespace ChooseC.MonacoEditor.Vue.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScriptController : ControllerBase
    {
        [HttpPost("format")]
        public dynamic Format()
        {
            try
            {
                string script = Request.Form["script"];
                return FormatCode(script);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [HttpPost("intellisense")]
        [Obsolete("not supports")]
        public dynamic IntelliSense()
        {
            try
            {
                string script = Request.Form["script"];
                return script;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        [HttpPost("validate")]
        public dynamic Validate()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var script = reader.ReadToEnd();
                CSScriptHelper.CheckScript(script, out Exception error);
                if (error == null) return "Pass";
                return error;
            }
            catch (Exception ex)
            {
                return ex;
            }

        }

        [HttpPost("execute")]
        public async Task<dynamic> Execute()
        {
            try
            {
                string script = Request.Form["script"].ToString();
                string inparam = Request.Form["inparam"];
                string methodname = Request.Form["methodname"];

                CSScriptHelper.CheckScript(script, out Exception error);
                if (error != null) return error;
                return CSScriptHelper.Execute(script, inparam, methodname);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private dynamic FormatCode(string code)
        {

            var assemblies = new[]
            {
                Assembly.Load("Microsoft.CodeAnalysis"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
            };

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()
                    .SelectMany(x => x.GetTypes())
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);
            var sourceLanguage = new CSharpLanguage();

            SyntaxTree syntaxTree = sourceLanguage.ParseText(code, SourceCodeKind.Script);
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            return Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, workspace).ToFullString();
        }
    }
    public static class CSScriptHelper
    {
        static CSScriptHelper()
        {
            CSScript.EvaluatorConfig.Engine = EvaluatorEngine.Roslyn;
            CSScript.EvaluatorConfig.Access = EvaluatorAccess.Singleton;
        }

        public static dynamic Execute(string script, string inparam, string methodname)
        {
            var instance = CSScript.Evaluator.LoadCode(script);
            return instance.GetType().GetMethod(methodname).Invoke(instance, new object[] { inparam });
        }

        public static void CheckScript(string script, out Exception? error)
        {
            error = null;
            try
            {
                CSScript.Evaluator.Check(script);
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }
    }
}
