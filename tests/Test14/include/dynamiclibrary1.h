/*
Copyright (c) 2010-2017, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#ifndef DYNAMICLIBRARY1_H
#define DYNAMICLIBRARY1_H

/* specific platform settings */
#if defined(_WIN32)
#define DYNAMICLIBRARY1_EXPORT __declspec(dllexport)
#define DYNAMICLIBRARY1_IMPORT __declspec(dllimport)
#elif __GNUC__ >= 4
#define DYNAMICLIBRARY1_EXPORT __attribute__ ((visibility("default")))
#endif

/* defaults */
#ifndef DYNAMICLIBRARY1_EXPORT
#define DYNAMICLIBRARY1_EXPORT /* empty */
#endif
#ifndef DYNAMICLIBRARY1_IMPORT
#define DYNAMICLIBRARY1_IMPORT /* empty */
#endif

#if defined(D_BAM_DYNAMICLIBRARY_BUILD)
#define DYNAMICLIBRARY1_API DYNAMICLIBRARY1_EXPORT
#else
#define DYNAMICLIBRARY1_API DYNAMICLIBRARY1_IMPORT
#endif

extern DYNAMICLIBRARY1_API int DynamicLibrary1Function(char);

extern DYNAMICLIBRARY1_API int DynamicLibrary1ExtraFunction();

#endif /* DYNAMICLIBRARY1_H */
