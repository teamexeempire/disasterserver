#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "export.h"
#include <QMainWindow>
#include <QCloseEvent>
#include <QListWidgetItem>
#include <vector>
#include <mutex>
#include <array>

QT_BEGIN_NAMESPACE
namespace Ui { class MainWindow; }
QT_END_NAMESPACE

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

    bool pollEvents(PollData* data);
    void queueEvent(PollData data);
    void log(QString string);
    void setMapList(std::array<bool, 16> array);
    void setPlayerState(PlayerData data);
    void addBan(QString name, QString ip);

protected:
    void closeEvent(QCloseEvent* event) override;

private slots:
    void on_comboBox_currentIndexChanged(int index);
    void on_mapSet_itemClicked(QListWidgetItem *item);

    void on_mapSetOpen_clicked();
    void on_mapSetSave_clicked();

    void on_playersContextMenu(const QPoint &);

    void on_player_kick();
    void on_player_ban();

    void on_banListDelete_clicked();

    void on_backToLobby_clicked();

    void on_exeWin_clicked();

    void on_pushButton_2_clicked();

private:
    void resetMapList();

    Ui::MainWindow *ui;
    std::vector<PollData> poll_queue;
    std::timed_mutex poll_mutex;
};
#endif // MAINWINDOW_H
