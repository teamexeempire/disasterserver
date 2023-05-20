#include "export.h"
#include "mainwindow.h"
#include <QApplication>
#include <thread>
#include <chrono>

QApplication* app = nullptr;
MainWindow* win = nullptr;
std::thread th;

// Functions used in C#
extern "C"
{
    #ifndef WIN32 // or something like that...
    #define __stdcall
    #endif
    typedef void (__stdcall *ReadyCallback)(void);

    EXPORT bool gui_poll_events(PollData* data)
    {
        if(!win || !app)
            return false;

        return win->pollEvents(data);
    }

    EXPORT void gui_log(const char* string)
    {
        if(!win || !app)
            return;

        win->log(QString::fromUtf16((const char16_t*)string));
    }

    EXPORT void gui_set_status(const char* string)
    {
        if(!win || !app)
            return;

        win->setStatusTip(QString::fromUtf16((const char16_t*)string));
    }

    EXPORT void gui_add_ban(const char* name, const char* ip)
    {
        if(!win || !app)
            return;

        win->addBan(QString::fromUtf16((const char16_t*)name), (QString::fromUtf16((const char16_t*)ip)));
    }

    EXPORT void gui_player_state(PlayerData data)
    {
        if(!win || !app)
            return;

        win->setPlayerState(data);
    }

    EXPORT int gui_run(ReadyCallback cb)
    {
        th = std::thread([=]
        {
            int argc = 1;
            char* argv[] = { (char*)"AppName" };
            app = new QApplication(argc, argv);
            win = new MainWindow();
            win->show();

            cb();

            while(true)
            {
                app->processEvents(QEventLoop::AllEvents | QEventLoop::WaitForMoreEvents);
                std::this_thread::sleep_for(std::chrono::milliseconds(10));
            }
        });

        return true;
    }
}

int main(int argc, char *argv[])
{
    app = new QApplication(argc, argv);
    win = new MainWindow();
    win->show();

    return app->exec();
}
