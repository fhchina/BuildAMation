GENCCODE_ASSEMBLY_TYPE=-a gcc
SO=so
SOBJ=so
A=a
LIBPREFIX=lib
LIB_EXT_ORDER=.51.2
COMPILE=gcc -D_REENTRANT   -DU_ATTRIBUTE_DEPRECATED= -O3 -Wall -std=c99 -pedantic -Wshadow -Wpointer-arith -Wmissing-prototypes -Wwrite-strings  -c
LIBFLAGS=-I/usr/local/include -DPIC -fPIC
GENLIB=gcc -O3 -Wall -std=c99 -pedantic -Wshadow -Wpointer-arith -Wmissing-prototypes -Wwrite-strings   -shared -Wl,-Bsymbolic
LDICUDTFLAGS=-nodefaultlibs -nostdlib
LD_SONAME=-Wl,-soname -Wl,
RPATH_FLAGS=
BIR_LDFLAGS=-Wl,-Bsymbolic
AR=ar
ARFLAGS=r
RANLIB=ranlib
INSTALL_CMD=/usr/bin/install -c