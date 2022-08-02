#require flash.fs

$00000010 constant RESET_Enable
$20000000 constant LOCKUP_RST_Enable
$3FFFFFFF constant USER_MEM
$00000000 constant USER_MEM_RAM32K_ROM96K
$40000000 constant USER_MEM_RAM64K_ROM64K
$80000000 constant USER_MEM_RAM96K_ROM32K
$00000800 constant BOOT_PIN_PA5
$FFFFF7FF constant BOOT_PIN_PA13
$FFF00FFF constant FLASH_WRProt

$04 constant WRProt_Size \ 4kB
WRProt_Size 12 lshift constant FLASH_WRProt_Size_4KB

: write-user-option-bytes ( options flashprot-size boot-pin-en -- success )
  >r \ tuck away bootpin enable
  >r \ tuck away flashprotection size
  0x14 rom-read-word      ( options config )
  $f7f9bf11 = if          ( options )
    1 6 lshift %101 or    ( options_r )
    dup 21 rshift
        7 lshift 8 and
        $700       and    ( options_r options_w )
    swap                  ( options_w options_r )
    $FFFFFF00 or
    dup 24 lshift not
        $ff       or          ( options_w options_r1 options_r2 )
    and                       ( options_w options_r )
    $FF0000FF and             ( options_w options_r )
    r> %101 or or             ( options_w options_r )
    swap                      ( options_r options_w )
    over                      ( options_r options_w options_r )
    or                        ( options_r options_w )
    r> if 1 lshift 11 or then ( options_r options_w )
    $14 rom-write-word        ( options_r )
    $14 rom-read-word         ( options_r options_r )
    = if 0 else 1 then
  else
    1
  then
;
