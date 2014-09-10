####################################################
###### Copyright(c) GHI Electronics, LLC ######
####################################################

OUTFILE=PWMGenerator
LINKERSCRIPT = PWMGenerator.lds

OUTPATH = ./Output
#OUTPATH = "../RLPExampleCSharp/FEZ-Panda-II-Application1/Resources"

INCL=./include

CC		=arm-none-eabi-gcc
LD		=arm-none-eabi-gcc

CCFLAGS=  -g -mlittle-endian -mcpu=arm7tdmi  -Wall -I. -I$(INCL)
CCFLAGS+= -mapcs-frame -fno-builtin -g0

LDFLAGS =-nostartfiles -Wl,--Map -Wl,$(OUTPATH)/$(OUTFILE).map
LDFLAGS+=-lc -lgcc -Wl,--omagic
LDFLAGS+=-T $(LINKERSCRIPT)

OBJS+= PWMGenerator.o

rebuild: clean all del_o

all: $(OBJS)
	$(LD) $(LDFLAGS) -o $(OUTPATH)/$(OUTFILE).elf $(OBJS)
	

PWMGenerator.o: PWMGenerator.c 
	$(CC) -c $(CCFLAGS) PWMGenerator.c -o PWMGenerator.o

clean:
	-rm PWMGenerator.o $(OUTPATH)/PWMGenerator.elf $(OUTPATH)/PWMGenerator.map

del_o:
	-rm PWMGenerator.o

del_map:
	-rm $(OUTPATH)/PWMGenerator.map
