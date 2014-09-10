/**********************************************************************************
* PulseCounter driver.
*
* Programmed by Huysentruit Wouter
* 2012 (c) Copyright by Fastload-Media.be
*
*/
#include "type.h"
#include "LPC23xx.h"
#include "RLP.h"
#include "irq.h"
#include "pwm.h"

#define MICROSECOND   18 

#define STATUS_SUCCESS                0
#define STATUS_ISR_INSTALL_FAILED    -1
#define STATUS_OUT_OF_MEMORY        -2
#define STATUS_MUST_WAIT            -3
#define STATUS_INVALID_ARG_COUNT    -4
#define STATUS_INVALID_FORMAT        -5

BOOL IsrInstalled = FALSE;

WORD Counters[4] = { 0, 0, 0, 0 };
WORD MsSinceLastCall = 0;

int Deinit(PVOID generalArray, void **args, unsigned int argsCount, unsigned int *argSize);

void TimerIsr(PVOID arg)
{
    ++MsSinceLastCall;
    
    // Reset MR0 interrupt
    T3IR |= 1;
}

// interrupt for counting rising edges of SPEED1 input
void GpioIsr(PVOID arg)
{
	do
	{
		if(IO0_INT_STAT_R & (1 << 9))
		{
			// SPEED1 counter
			IO0_INT_CLR |= (1 << 9);
			Counters[0]++;
		}
		if(IO0_INT_STAT_R & (1 << 8))
		{
			// SPEED2 counter
			IO0_INT_CLR |= (1 << 8);
			Counters[1]++;
		}
		if(IO0_INT_STAT_R & (1 << 7))
		{
			// SPEED3 counter
			IO0_INT_CLR |= (1 << 7);
			Counters[2]++;
		}
	} while(IO0_INT_STAT_R & ((1 << 7) | (1 << 8) | (1 << 9)));
}

int Init(unsigned int *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
    Deinit(NULL, NULL, 0, NULL);

    // Setup timer 3
    PCONP |= 1 << 23;			// Power timer 3
    PCLKSEL1 |= 0x03 << 14;		// T3 clock = PCLK / 8 = 72 Mhz / 8 = 9 Mhz
	
    T3MR0 = (9000000 / 1000) - 1;    // 1000 Hz
    
    T3MCR = 0x03;				// bit0: Interrupt on MR0: an interrupt is generated when MR0 matches the value in the TC
								// bit1: Reset on MR0: the TC will be reset if MR0 matches it
    T3PR = 0;					// Set prescaler
    T3TCR = 1;					// Start timer
    
    // Setup GPIO's for edge interrupt
	PINSEL0 &= ~((3 << 14) | (3 << 16) | (3 << 18));	// P0.7, P0.8, P0.9 are GPIO
	PINMODE0 &= ~((3 << 14) | (3 << 16) | (3 << 18));	// P0.7, P0.8, P0.9 pulled up
	SCS |= 1;											// fast GPIO mode for P0 and P1
	FIO0DIR &= ((1 << 7) | (1 << 8) | (1 << 9));		// P0.7, P0.8, P0.9 are inputs
	IO0_INT_CLR |= ((1 << 7) | (1 << 8) | (1 << 9));	// clear any pending interrupts on these inputs
	IO0_INT_EN_R |= ((1 << 7) | (1 << 8) | (1 << 9));	// rising edge interrupt on P0.7, P0.8, P0.9
	IO0_INT_EN_F &= ~((1 << 7) | (1 << 8) | (1 << 9));	// no falling edge interrupts on P0.7, P0.8, P0.9

    // Hook our interrupt handlers
    if (!RLPext->Interrupt.Install(TIMER3_INT, TimerIsr, NULL))
        return STATUS_ISR_INSTALL_FAILED;
    
	if (!RLPext->Interrupt.Install(EINT3_INT, GpioIsr, NULL))
        return STATUS_ISR_INSTALL_FAILED;
  
    IsrInstalled = TRUE;

    return STATUS_SUCCESS;
}

int Deinit(PVOID generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
    if (IsrInstalled)
    {
        RLPext->Interrupt.Uninstall(TIMER3_INT);
        RLPext->Interrupt.Uninstall(EINT3_INT);
        IsrInstalled = FALSE;
    }

    return STATUS_SUCCESS;
}

int Query(PWORD generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
    WORD i;
    
    RLPext->Interrupt.Disable(TIMER3_INT);
    RLPext->Interrupt.Disable(EINT3_INT);
    generalArray[0] = MsSinceLastCall;
    MsSinceLastCall = 0;
    for (i = 0; i < 3; i++)
    {
        generalArray[i + 1] = Counters[i];
        Counters[i] = 0;
    }
    RLPext->Interrupt.Enable(TIMER3_INT);
    RLPext->Interrupt.Enable(EINT3_INT);
        
    return STATUS_SUCCESS;
}

int PwmStart(unsigned int frequencyHz, float width)
{
	PINSEL3 |= 0x008A00;	/* set GPIOs for all PWM2 & PWM3 */
 
    PWM1TCR = TCR_RESET;	/* Counter Reset */ 
 
    PWM1PR = 0x00;			/* count frequency:Fpclk */
    PWM1MCR = PWMMR0R;	    /* reset on PWMMR0, reset TC if PWM0 matches */				
 
    long CYCLE_WIDTH = (1000000 / frequencyHz) * MICROSECOND;
    PWM1MR0 = CYCLE_WIDTH;		/* set PWM cycle */
 
	long rate = (long)(width * MICROSECOND);
    PWM1MR2 = rate;
    PWM1MR3 = rate * 3;
 
    /* PWM latches enabled, ready to start */
    PWM1LER = LER0_EN | LER2_EN | LER3_EN;
 
    /* PWM2 single edge, PWM3 double edge, all enable */
    PWM1PCR = PWMENA2 | PWMSEL3 | PWMENA3;
    PWM1TCR = TCR_CNT_EN | TCR_PWM_EN;	    /* counter enable, PWM enable */
 
	return (TRUE);
}
 
void PwmStop()
{
    PWM1PCR = 0;
    PWM1TCR = 0x00;		/* Stop all PWMs */
}
 
int PWMBlink(unsigned int *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	unsigned int frequencyHz = *(unsigned int*)args[0];
	float width = *(float*)args[1];
	/*unsigned long microseconds = *(long*)args[2];*/
 
	PwmStart(frequencyHz, width);
	/*RLPext->Delay(microseconds);*/
	/*PwmStop();*/
 
	return 0;
}

int PWMUpdateFreq(unsigned int *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	unsigned int frequencyHz = *(unsigned int*)args[0];

	long CYCLE_WIDTH = (1000000 / frequencyHz) * MICROSECOND;
    PWM1MR0 = CYCLE_WIDTH;		// update PWM cycle
	PWM1LER |= LER0_EN;			// latch match0 update

	return 0;
}

int PWMUpdatePulse(unsigned int *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	float width = *(float*)args[0];

	long rate = (long)(width * MICROSECOND);
    PWM1MR2 = rate;
    PWM1MR3 = rate * 3;

	PWM1LER |= (LER2_EN | LER3_EN);			// latch match1 and match2 update

	return 0;
}

void PWMRun()
{
	PWM1TCR = TCR_RESET;	/* Counter Reset */ 

	/* PWM2 single edge, PWM3 double edge, all enable */
    PWM1PCR = PWMENA2 | PWMSEL3 | PWMENA3;
    PWM1TCR = TCR_CNT_EN | TCR_PWM_EN;	    /* counter enable, PWM enable */
}

int QueryPWMStatus(PWORD generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if(PWM1PCR)
		return TRUE;
	else
		return FALSE;
}

void WDReset()
{
	// diasble interrupts
	RLPext->Interrupt.Disable(TIMER3_INT);
    RLPext->Interrupt.Disable(EINT3_INT);

	WDCLKSEL = 0x0;							// internal RTC oscillator clock source
	WDTC = 0xFF;							// minimum watchdog counter
	WDMOD = 0x3;							// reset on watchdog timeout
	WDFEED = 0xAA;
	WDFEED = 0x55;							// force immediate reset
}

void PPS_Interrupt ( unsigned int Pin, unsigned int PinState, void* Param )
{
    RLPext->PostManagedEvent(0);
}
 
int InitPPS(unsigned int *generalArray, void **args, unsigned int argsCount, unsigned int *argData)
{
    RLP_InterruptInputPinArgs Args;
    Args.GlitchFilterEnable  = RLP_TRUE; // Enable Glitch Filter
    Args.IntEdge = RLP_GPIO_INT_EDGE_HIGH; // Interrupt on Rising Edge
    Args.ResistorState = RLP_GPIO_RESISTOR_PULLUP; // Enable internal pull-up resistor
    RLPext->GPIO.EnableInterruptInputMode(
       66,            // unsigned int Pin, 
       &Args,         // RLP_InterruptInputPinArgs *args, 
       PPS_Interrupt, // RLP_GPIO_INTERRUPT_SERVICE_ROUTINE ISR, 
       (void*)0       // void* ISR_Param
    );
    return 0;
}
