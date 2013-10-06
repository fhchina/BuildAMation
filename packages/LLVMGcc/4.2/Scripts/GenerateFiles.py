#!/usr/bin/python

import os
import sys
from optparse import OptionParser

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# C compiler options
cCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "LLVMGcc", "4.2", "Scripts", "ICCompilerOptions.cs")),
    "-n=LLVMGcc",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e" # this option set derives from the GccCommon option set
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=LLVMGcc",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e" # this option set derives from the C option set
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout

# ObjC compiler options
objcCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "LLVMGcc", "4.2", "Scripts", "ICCompilerOptions.cs")),
    "-n=LLVMGcc",
    "-c=ObjCCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e" # this option set derives from the GccCommon option set
]
objcCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(objcCompiler_options, True, True)
print stdout

# C++ compiler options
objcxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=LLVMGcc",
    "-c=ObjCxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "XcodeProjectProcessor", "dev", "Scripts", "Delegate.cs")),
    "-pv=GccCommon.PrivateData",
    "-e" # this option set derives from the C option set
]
objcxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(objcxxCompiler_options, True, True)
print stdout