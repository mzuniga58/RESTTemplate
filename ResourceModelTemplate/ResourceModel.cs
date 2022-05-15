using System;
using Tense;$if$ ($userql$ == True)
using Tense.Rql;$endif$
using Microsoft.AspNetCore.Mvc.ModelBinding;
using $orchestrationnamespace$;
$if$ ($resourcebarray$ == true)using System.Collections;
$endif$using System.Collections.Generic;
$if$ ($usenpgtypes$ == true)using NpgsqlTypes;
$endif$$if$ ($resourceimage$ == true)using System.Drawing;
$endif$$if$ ($resourcenet$ == true)using System.Net;
$endif$$if$ ($resourcenetinfo$ == true)using System.Net.NetworkInformation;
$endif$using System.ComponentModel.DataAnnotations;
using $entitynamespace$;

namespace $rootnamespace$
{
$model$}
