using System.Reflection;
using System.Runtime.CompilerServices;

// The culture of the assembly.

[assembly: AssemblyCulture(@"")]

// The description of the assembly.

[assembly: AssemblyDescription(@"Contains types that extends functionality of Microsoft Azure.")]

// Defines a product name custom attribute for an assembly manifest.

[assembly: AssemblyProduct(@"Azure Extensions")]

// The title of the assembly.

[assembly: AssemblyTitle(@"SXN.Azure")]

#if TEST

// The test assembly.

[assembly: InternalsVisibleTo(@"SXN.Azure.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001004fa8962359b602683d3b7db4a0afcc8786caa860dd02689e2efa2671eebcc763401164315039827a67662e56a2defa4c7f612a9454e4a3e3c9823054155b22b1983f85a52652cd4c6497349fb461baaa35cdd5bf62df0a41e7b3ffbffe6551432805ca976b795e5134152680162c528db50b694b66b31a9cc2ca6f3cdad0f79a")]

#endif