#ifndef EXPORT_H
#define EXPORT_H
#include <cstdint>

#if defined(_MSC_VER)
    //  Microsoft
    #define EXPORT __declspec(dllexport)
    #define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
    //  GCC
    #define EXPORT __attribute__((visibility("default")))
    #define IMPORT
#else
    //  do nothing and hope for the best?
    #define EXPORT
    #define IMPORT
    #pragma warning Unknown dynamic link import/export semantics.
#endif

struct PollData
{
    uint16_t value1;

    enum
    {
        POLL_NONE,
        POLL_QUIT,

        POLL_CLEAR_EXCLUDES,
        POLL_ADD_EXCLUDE,

        POLL_KICK,
        POLL_BAN,
        POLL_UNBAN,
        POLL_BACKTOLOBBY,
        POLL_EXEWIN,
        POLL_SURVWIN
    } type;

    char value2[256];
};

struct PlayerData
{
    int32_t state;
    uint16_t pid;
    char name[256];

    enum
    {
        CHARACTER_NONE,

        CHRACTER_TAILS,
        CHARACTER_KNUX,
        CHARACTER_EGGMAN,
        CHARACTER_AMY,
        CHARACTER_SALLY,
        CHARACTER_CREAM,

        CHARACTER_EXE,
        CHARACTER_CHAOS,
        CHARACTER_EXETIOR,
        CHARACTER_EXELLER
    } character;
};

#endif // EXPORT_H
