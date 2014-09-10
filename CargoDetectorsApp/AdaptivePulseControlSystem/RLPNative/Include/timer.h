/*****************************************************************************
 *   timer.h:  Header file for NXP LPC23xx/24xx Family Microprocessors
 *
 *   Copyright(C) 2006, NXP Semiconductor
 *   All rights reserved.
 *
 *   History
 *   2006.07.13  ver 1.00    Prelimnary version, first Release
 *
******************************************************************************/
#ifndef __TIMER_H 
#define __TIMER_H

/* depending on the CCLK and PCLK setting, e.g. CCLK = 60Mhz, 
PCLK = 1/4 CCLK, then, 10mSec = 150.000-1 counts */
// #define TIME_INTERVAL	149999
	
#define TIME_INTERVAL	(Fpclk/100 - 1)

extern void delayMs(BYTE timer_num, DWORD delayInMs);
extern DWORD init_timer( BYTE timer_num, DWORD timerInterval );
extern void enable_timer( BYTE timer_num );
extern void disable_timer( BYTE timer_num );
extern void reset_timer( BYTE timer_num );
extern void Timer0FIQHandler( void );
extern void Timer1FIQHandler( void );

#endif /* end __TIMER_H */
/*****************************************************************************
**                            End Of File
******************************************************************************/
