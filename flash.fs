#require global.fs

\ internal SPI Flash register
$40001014 constant R32_ROM_DATA
$40001018 constant R32_ROM_CTRL
$40001018 constant R8_ROM_DATA   \ RW
$4000101A constant R16_ROM_CR    \ RW

$8000 constant ROM_ADDR_OFFSET

\ internal SPI Flash register address offset and bit defines
0 constant ROM_DATA
1 constant ROM_CR
1 constant ROM_SCS
2 constant ROM_SIO0_OE
4 constant ROM_RD_EN
8 constant ROM_WR8
0 constant ROM_DES_LONG

: _rom-data-write ( databyte -- )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  R8_ROM_DATA c!
;

: _rom-data-read ( -- databyte )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  R8_ROM_DATA c@
;

: _rom-access ( val -- )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  ( val ) R16_ROM_CR c!
;

: _rom-access-end ( -- )
  0 _rom-access
;

%1011 constant _ROM_BEGIN_READ
%0110 constant _ROM_BEGIN_WRITE
%0101 constant _ROM_END_WRITE

: _rom-begin ( code -- )
  0     R16_ROM_CR   c!
  %111  R16_ROM_CR   c!
        R32_ROM_CTRL c!
;

: _rom-write-addr ( addr -- )
  dup 16 rshift $ff and _rom-data-write
  dup 8  rshift $ff and _rom-data-write
                $ff and _rom-data-write
;

: _rom-read-byte ( -- byte )
    _rom-data-read drop
    _rom-data-read drop
    _rom-data-read drop
    R8_ROM_DATA c@
;

: rom-read-byte ( addr -- byte )
  ROM_ADDR_OFFSET +
  _ROM_BEGIN_READ _rom-begin
  _rom-write-addr
  _rom-read-byte
  _rom-access-end
;

: rom-read-word ( addr -- word )
  ROM_ADDR_OFFSET + ( addr )
  _ROM_BEGIN_READ _rom-begin
  _rom-write-addr
  _rom-data-read drop _rom-data-read drop
  _rom-read-byte
  _rom-read-byte  8 lshift or
  _rom-read-byte 16 lshift or
  _rom-read-byte 24 lshift or
  _rom-access-end
;


: _rom-program-start ( code -- )
  _ROM_BEGIN_WRITE _rom-begin
  _rom-access-end
  _rom-begin
;

: _rom-write-start ( -- )
  %10 _rom-program-start
;

: _rom-erase-4k-start ( -- )
  $20 _rom-program-start
;

: _rom-erase-64k-start ( -- )
  $d8 _rom-program-start
;

: _rom-write-end ( -- status )
  _rom-access-end
  $ff >r \ return status
  $280000 0
  do
    _ROM_END_WRITE _rom-begin
    _rom-data-read drop
    _rom-data-read ( status )
    _rom-access-end
    1 and if r> drop 0 >r leave then
  loop
  r>
;

: _rom-write-enable
  safe-access-mode-on
  R8_GLOB_ROM_CFG c@
  ROM_DATA_WE ROM_CODE_WE ROM_WRITE or or or
  R8_GLOB_ROM_CFG c!
;

: _rom-write-disable
  safe-access-mode-on
  R8_GLOB_ROM_CFG c@
  ROM_DATA_WE ROM_CODE_WE or not and
  ROM_WRITE or
  R8_GLOB_ROM_CFG c!
;

: rom-write-word ( data addr --  )
  %11 not and ROM_ADDR_OFFSET + ( data romaddr -- )
  dup $80000 < if
    _rom-write-enable
    begin
      _rom-write-start     ( data romaddr )
      _rom-write-addr      ( data )
      R32_ROM_DATA !       (      )
      R16_ROM_CR c@ $10 or ( val  )
      dup _rom-access
      dup _rom-access
      dup _rom-access
          _rom-access
      _rom-write-end
    0<> until
    _rom-write-disable
  else
    ." address out of range" cr
  then
;

\ $1000 = 4kB
: rom-erase-4k ( addr --  )
  $fff not and ROM_ADDR_OFFSET + ( romaddr -- )
  dup $80000 < if
    _rom-write-enable
    _rom-erase-4k-start
    _rom-write-addr
    _rom-write-end
    _rom-write-disable
  else
    ." address out of range" cr
  then
;

\ $10000 = 64kB
: rom-erase-64k ( addr --  )
  $ffff not and ROM_ADDR_OFFSET + ( romaddr -- )
  dup $80000 < if
    _rom-write-enable
    _rom-erase-64k-start
    _rom-write-addr
    _rom-write-end
    _rom-write-disable
  else
    ." address out of range" cr
  then
;