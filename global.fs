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

$40001005 constant R8_RST_BOOT_STAT \ RO, reset status and boot/debug status, bit7:6 always are 1:1
$00 constant RST_FLAG_SW   \ 00 - software reset, by RB_SOFTWARE_RESET=1 @(RB_BOOT_LOADER=0 or RB_WDOG_RST_EN=1), or set reset request from PFIC
$01 constant RST_FLAG_POR  \ 01 - power on reset
$02 constant RST_FLAG_WDOG \ 10 - watch-dog timer overflow reset, or CORE LOCKUP reset
$03 constant RST_FLAG_PIN  \ 11 - external manual reset by RST# pin input low\

$04 constant CFG_RESET_EN  \ RO, manual reset input enable status
$08 constant CFG_BOOT_EN   \ RO, boot-loader enable status
$10 constant CFG_DEBUG_EN  \ RO, debug enable status
$20 constant BOOT_LOADER   \ RO, indicate boot loader status: 0=application status (by software reset), 1=boot loader status

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

: safe-access-mode-on ( -- )
  $57 R8_SAFE_ACCESS c!
  $a8 R8_SAFE_ACCESS c!
;

: delay ( ms -- )
  $2ee0 * \ 12000
  begin 1- dup 0= until
  drop
;

: cls
  $1b emit ." [2J"       \ clear screen
  $1b emit ." [0;0H" cr  \ position cursor at (0,0)
;