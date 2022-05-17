using $orchestrationnamespace$;
using $resourcenamespace$;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Mime;
using Tense;$if$ ($userql$ == True)
using Tense.Rql;$endif$
$if$ ($policy$ != none)using Microsoft.AspNetCore.Authorization;
$endif$

namespace $rootnamespace$
{
$model$}
