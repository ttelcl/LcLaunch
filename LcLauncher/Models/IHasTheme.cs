using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.Models;

public interface IHasTheme
{
  /// <summary>
  /// The theme name
  /// </summary>
  string Theme { get; set; }
}
