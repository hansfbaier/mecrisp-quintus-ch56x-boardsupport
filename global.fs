#require condcomp.fs

$20000000 constant RAMS
$20020000 constant RAMX

$40001000 constant R8_SAFE_ACCESS
$40001001 constant R8_CHIP_ID       \ chip ID
$69       constant CH569

$40001004 constant R8_GLOB_ROM_CFG  \ RWA, flash ROM configuration, SAM and bit7:6 must write 1:0

1 0 lshift constant ROM_EXT_RE      \ RWA, enable flash ROM being read by external programmer: 0=reading protect, 1=enable read
1 1 lshift constant CODE_RAM_WE     \ RWA, enable code RAM being write: 0=writing protect, 1=enable write
1 2 lshift constant ROM_DATA_WE     \ RWA, enable flash ROM data area being erase/write: 0=writing protect, 1=enable program and erase
1 3 lshift constant ROM_CODE_WE     \ RWA, enable flash ROM code & data area being erase/write: 0=writing protect, 1=enable program and erase
1 4 lshift constant ROM_CODE_OFS    \ RWA, user code offset in ROM: 1: 0x04000; 0: 0x00000.
1 7 lshift constant ROM_WRITE  \ don't know the function for sure, must be set for writing flash

$40001006 constant R8_RST_WDOG_CTRL

\ the variable lite needs to be defined to strip
\ down the included code to the minimum
[ifndef] lite

: GLOB-ROM-CFG.
  R8_GLOB_ROM_CFG c@
  dup ROM_EXT_RE   and if cr ." ROM_EXT_RE" then
  dup CODE_RAM_WE  and if cr ." CODE_RAM_WE" then
  dup ROM_DATA_WE  and if cr ." ROM_DATA_WE" then
  dup ROM_CODE_WE  and if cr ." ROM_CODE_WE" then
  dup ROM_CODE_OFS and if cr ." ROM_CODE_OFS" then
  dup ROM_WRITE    and if cr ." ROM_WRITE" then
  drop
  cr
;

[then]

$40001005 constant R8_RST_BOOT_STAT \ RO, reset status and boot/debug status, bit7:6 always are 1:1
$00 constant RST_FLAG_SW   \ 00 - software reset, by RB_SOFTWARE_RESET=1 @(RB_BOOT_LOADER=0 or RB_WDOG_RST_EN=1), or set reset request from PFIC
$01 constant RST_FLAG_POR  \ 01 - power on reset
$02 constant RST_FLAG_WDOG \ 10 - watch-dog timer overflow reset, or CORE LOCKUP reset
$03 constant RST_FLAG_PIN  \ 11 - external manual reset by RST# pin input low\

$04 constant CFG_RESET_EN  \ RO, manual reset input enable status
$08 constant CFG_BOOT_EN   \ RO, boot-loader enable status
$10 constant CFG_DEBUG_EN  \ RO, debug enable status
$20 constant BOOT_LOADER   \ RO, indicate boot loader status: 0=application status (by software reset), 1=boot loader status

[ifndef] lite

: RST-BOOT-STAT.
  R8_RST_BOOT_STAT c@
  dup %11 and case
    RST_FLAG_SW   of cr ." RST_FLAG_SW:   software reset" endof
    RST_FLAG_POR  of cr ." RST_FLAG_POR:  power on reset" endof
    RST_FLAG_WDOG of cr ." RST_FLAG_WDOG: watch-dog timer or core lockup reset" endof
    RST_FLAG_PIN  of cr ." RST_FLAG_PIN:  reset pin low" endof
  endcase
  dup CFG_RESET_EN  and if cr ." CFG_RESET_EN:  manual reset input enabled" then
  dup CFG_BOOT_EN   and if cr ." CFG_BOOT_EN:   boot-loader enabled" then
  dup CFG_DEBUG_EN  and if cr ." CFG_DEBUG_EN:  debug enable active" then
  dup BOOT_LOADER   and if cr ." BOOT_LOADER:   boot loader active" then
  drop
  cr
;

[then]

$E000E000 constant PFIC
$E000F000 constant SysTick

\ $FA050000 constant PFIC_KEY1
\ $BCAF0000 constant PFIC_KEY2
\ $BEEF0000 constant PFIC_KEY3

PFIC $0   + constant R32_PFIC_ISR       \ PFIC interrupt enable status register
PFIC $20  + constant R32_PFIC_IPR       \ PFIC interrupt suspend status register
PFIC $40  + constant R32_PFIC_ITHRESDR  \ PFIC interrupt priority threshold configuration register
PFIC $44  + constant R32_PFIC_CFGR      \ PFIC interrupt configuration register
PFIC $4C  + constant R32_PFIC_GISR      \ PFIC interrupt global status register
PFIC $100 + constant R32_PFIC_IENR      \ PFIC interrupt enable setting register
PFIC $180 + constant R32_PFIC_IRER      \ PFIC interrupt enable reset register

#1  constant  Reset_IRQn
#2  constant  NMI_IRQn
#3  constant  EXC_IRQn
#12 constant SysTick_IRQn
#14 constant SWI_IRQn
#16 constant WDOG_IRQn
#17 constant TMR0_IRQn
#18 constant GPIO_IRQn
#19 constant SPI0_IRQn
#20 constant USBSS_IRQn
#21 constant LINK_IRQn
#22 constant TMR1_IRQn
#23 constant TMR2_IRQn
#24 constant UART0_IRQn
#25 constant USBHS_IRQn
#26 constant EMMC_IRQn
#27 constant DVP_IRQn
#28 constant HSPI_IRQn
#29 constant SPI1_IRQn
#30 constant UART1_IRQn
#31 constant UART2_IRQn
#32 constant UART3_IRQn
#33 constant SERDES_IRQn
#34 constant ETH_IRQn
#35 constant PMT_IRQn
#36 constant ECDC_IRQn

[ifndef] lite

: interrupt-name. ( irqnum -- )
  case
     #1 of ." Reset_IRQn"   endof
     #2 of ." NMI_IRQn"     endof
     #3 of ." EXC_IRQn"     endof
    #12 of ." SysTick_IRQn" endof
    #14 of ." SWI_IRQn"     endof
    #16 of ." WDOG_IRQn"    endof
    #17 of ." TMR0_IRQn"    endof
    #18 of ." GPIO_IRQn"    endof
    #19 of ." SPI0_IRQn"    endof
    #20 of ." USBSS_IRQn"   endof
    #21 of ." LINK_IRQn"    endof
    #22 of ." TMR1_IRQn"    endof
    #23 of ." TMR2_IRQn"    endof
    #24 of ." UART0_IRQn"   endof
    #25 of ." USBHS_IRQn"   endof
    #26 of ." EMMC_IRQn"    endof
    #27 of ." DVP_IRQn"     endof
    #28 of ." HSPI_IRQn"    endof
    #29 of ." SPI1_IRQn"    endof
    #30 of ." UART1_IRQn"   endof
    #31 of ." UART2_IRQn"   endof
    #32 of ." UART3_IRQn"   endof
    #33 of ." SERDES_IRQn"  endof
    #34 of ." ETH_IRQn"     endof
    #35 of ." PMT_IRQn"     endof
    #36 of ." ECDC_IRQn"    endof
  endcase
;

: interrupts. ( -- )
  R32_PFIC_IENR
  dup         ( intreg intreg )
  @
  12 rshift
  31 12 do
    dup 1 and if
      i interrupt-name. space
    then
    1 rshift
  loop
  drop
  4 + @
  27 0 do
    dup 1 and if
      i 32 + interrupt-name. space
    then
    1 rshift
  loop
  drop
;

[then]

: pfic-enable-irq ( irqn -- )
  dup 31 <= if
    1 swap   ( 1 irqn )
    lshift
    R32_PFIC_IENR !
  else
    32 -
    1 swap
    lshift
    R32_PFIC_IENR 4 + !
  then
;

\ TODO: currently only works for interrupts up to no.31
: pfic-disable-irq ( irqn -- )
  R32_PFIC_ITHRESDR @ >r   ( irqn r: ithresdr-val )
  $10 R32_PFIC_ITHRESDR !
  dup 31 <= if
    1 swap   ( 1 irqn )
    lshift
    R32_PFIC_IRER !
  else
    32 -
    1 swap
    lshift
    R32_PFIC_IRER 4 + !
  then
  r> R32_PFIC_ITHRESDR !
;

: safe-access-mode-on ( -- )
  $57 R8_SAFE_ACCESS c!
  $a8 R8_SAFE_ACCESS c!
;

[ifndef] MS_CYCLES
\ 1 ms @ 120MHz
#9984 constant MS_CYCLES
[then]

[ifndef] ms
: ms ( ms -- )
  MS_CYCLES *
  begin 1- dup 0= until
  drop
;
[then]

[ifndef] US_CYCLES
\ 1 ms @ 120MHz
#3 constant US_CYCLES
[then]

[ifndef] us
: us ( us -- )
  US_CYCLES *
  begin 1- dup 0= until
  drop
;
[then]

: cls
  $1b emit ." [2J"       \ clear screen
  $1b emit ." [0;0H" cr  \ position cursor at (0,0)
;