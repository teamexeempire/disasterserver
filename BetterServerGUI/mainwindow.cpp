#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <QDebug>
#include <QTimer>
#include <QFile>
#include <QFileDialog>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonArray>
#include <QMessageBox>
#include <QPointF>
#include <QAction>
#include <chrono>
#include <functional>

void dispatchToMainThread(std::function<void()> callback)
{
    // any thread
    QTimer* timer = new QTimer();
    timer->moveToThread(qApp->thread());
    timer->setSingleShot(true);
    QObject::connect(timer, &QTimer::timeout, [=]()
                     {
                         callback();
                         timer->deleteLater();
                     });
    QMetaObject::invokeMethod(timer, "start", Qt::QueuedConnection, Q_ARG(int, 0));
}

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    ui->players->setContextMenuPolicy(Qt::CustomContextMenu);
    connect(ui->players, SIGNAL(customContextMenuRequested(const QPoint &)), this, SLOT(on_playersContextMenu(const QPoint &)));

    ui->mapSet->setContextMenuPolicy(Qt::CustomContextMenu);
    connect(ui->mapSet, SIGNAL(customContextMenuRequested(const QPoint &)), this, SLOT(on_mapsContextMenu(const QPoint &)));

    connect(ui->actionAbout_QT, &QAction::triggered, [=](bool a)
            {
                QMessageBox::aboutQt(this);
            });


    connect(ui->actionAbout_BetterServer, &QAction::triggered, [=](bool a)
            {
                QMessageBox::about(this, "About BetterServer", "BetterServer is a complete rewrite of original DisasterServer.\n\n(c) 2023 Team Exe Empire");
            });


    connect(ui->actionExit, &QAction::triggered, [=](bool a)
            {
                queueEvent(PollData { 0, PollData::POLL_QUIT });
                this->close();
            });

    resetMapList();
}

MainWindow::~MainWindow()
{
    delete ui;
}

bool MainWindow::pollEvents(PollData* data)
{
    poll_mutex.try_lock_for(std::chrono::seconds(1));
    if(!poll_queue.empty())
    {
        memcpy(data, &poll_queue[0], sizeof(PollData));
        poll_queue.erase(poll_queue.begin());

        poll_mutex.unlock();
        return true;
    }

    poll_mutex.unlock();
    return false;
}

void MainWindow::queueEvent(PollData data)
{
    poll_mutex.try_lock_for(std::chrono::seconds(1));
    poll_queue.push_back(data);
    poll_mutex.unlock();
}

void MainWindow::log(QString string)
{
    dispatchToMainThread([this, string]
    {
        ui->logText->appendPlainText(string);
        ui->logText->ensureCursorVisible();
    });
}

void MainWindow::closeEvent(QCloseEvent *event)
{
    queueEvent(PollData { 0, PollData::POLL_QUIT });
}

void MainWindow::setMapList(std::array<bool, 18> array)
{
    resetMapList();

    for(quint16 i = 0; i < array.size(); i++)
    {
        if(!array[i])
            queueEvent(PollData { i , PollData::POLL_ADD_EXCLUDE });

        ui->mapSet->item(i)->setCheckState(array[i] ? Qt::Checked : Qt::Unchecked);
    }
}

void MainWindow::resetMapList()
{
    ui->mapSet->clear();
    queueEvent({ 0, PollData::POLL_CLEAR_EXCLUDES });

    const char* arr[] = { "Hide and Seek Act 2", "Ravine Mist", "...", "Desert Town", "You Can't Run", "Limp City", "Not Perfect", "Kind And Fair", "Act 9", "Nasty Paradise", "Priceless Freedom", "Volcano Valley", "Hill", "Majin Forest", "Hide and Seek", "Torture Cave", "Dark Tower", "Haunting Dream" };
    for(const char* str : arr)
    {
        QListWidgetItem* item = new QListWidgetItem(str, ui->mapSet);
        item->setFlags(item->flags() | Qt::ItemIsUserCheckable);
        item->setCheckState(Qt::Checked);
        ui->mapSet->addItem(item);
    }
}

void MainWindow::setPlayerState(PlayerData data)
{
    dispatchToMainThread([this, data]
    {
        switch(data.state)
        {
            case 0:
            {
                auto str = QString(QString::fromUtf16((char16_t*)data.name));
                QListWidgetItem* item = new QListWidgetItem(str, ui->players);
                item->setData(Qt::UserRole, (int)data.pid);
                ui->players->addItem(item);
                break;
            }

            case 1:
            {
                for(quint16 i = 0; i < ui->players->count(); i++)
                {
                    QListWidgetItem* item = ui->players->item(i);
                    auto pid = item->data(Qt::UserRole).toInt();

                    if(data.pid == pid)
                    {
                        delete item;
                        break;
                    }
                }
                break;
            }
        }
    });
}

void MainWindow::on_comboBox_currentIndexChanged(int index)
{
    switch(index)
    {
    case 0:
        setMapList(std::array<bool, 18> { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true });
        break;

    case 1:
        //                                 hs2   rmz    ...   dt     ycr   limp   np    kaf   act9   nap   price  volcan hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { false, false, true, false, true, false, true, true, true, false, false, false, true, false, true, false, false, false });
        break;

    case 2:
        //                                 hs2   rmz    ...   dt     ycr   limp   np    kaf   act9   nap   price  volcan   hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { true, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true });
        break;

    case 3:
        //                                 hs2   rmz    ...   dt     ycr   limp   np    kaf   act9   nap   price  volcan  hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { false, true, false, true, true, false, false, true, false, false, false, true, true, false, true, false, false, false });
        break;

    case 4:
        //                                 hs2   rmz     ...    dt     ycr   limp   np    kaf   act9   nap   price  volcan  hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { false, false, false, false, false, true, true, false, false, true, false, false, false, false, false, true, false, false });
        break;

    case 5:
        //                                 hs2   rmz     ...    dt     ycr    limp   np     kaf   act9   nap  price  volcan  hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { false, false, false, false, false, false, false, false, true, false, true, false, false, true, false, false, false, false });
        break;

    case 6:
        //                                 hs2   rmz     ...    dt    ycr    limp    np   kaf   act9   nap  price  volcan  hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { false, true, false, true, false, false, false, false, false, true, true, false, false, false, false, false, false, false });
        break;

    case 7:
        //                                 hs2   rmz     ...    dt    ycr   limp    np    kaf   act9   nap   price  volcan  hill  majon   hs1  torture
        setMapList(std::array<bool, 18> { true, false, false, false, false, true, false, false, false, false, false, true, false, true, false, false, false, false });
        break;
    }
}


void MainWindow::on_mapSet_itemClicked(QListWidgetItem *item)
{
    ui->comboBox->setCurrentIndex(8);
    queueEvent(PollData { 0, PollData::POLL_CLEAR_EXCLUDES });

    for(quint16 i = 0; i < ui->mapSet->count(); i++)
    {
        QListWidgetItem* item = ui->mapSet->item(i);

        if(item->checkState() == Qt::Unchecked)
            queueEvent(PollData { i , PollData::POLL_ADD_EXCLUDE });
    }
}


void MainWindow::on_mapSetOpen_clicked()
{
    QString filename = QFileDialog::getOpenFileName(this, "Open Map List", "", tr("MapList (*.mlist)"));

    if (filename.isEmpty())
        return;

    QFile file { filename };
    if (!file.open(QIODevice::ReadOnly))
    {
        QMessageBox box;
        box.setIcon(QMessageBox::Critical);
        box.setText("Couldn't open a file.");
        box.exec();
        return;
    }

    QJsonDocument json = QJsonDocument::fromJson(file.readAll());
    file.close();

    if(!json.isArray())
    {
        QMessageBox box;
        box.setIcon(QMessageBox::Critical);
        box.setText("Incorrect format. (Not a JSON array)");
        box.exec();
        return;
    }

    QJsonArray jObj = json.array();
    std::array<bool, 18> arr;

    int i = 0;
    for(auto it : jObj)
    {
        if(!it.isBool())
        {
            QMessageBox box;
            box.setIcon(QMessageBox::Critical);
            box.setText("Incorrect format. (Should be BOOL)");
            box.exec();
            return;
        }

        arr[i] = it.toBool();
        i++;
    }

    setMapList(arr);
    ui->comboBox->setCurrentIndex(8);
}


void MainWindow::on_mapSetSave_clicked()
{
    QString filename = QFileDialog::getSaveFileName(this, "Save Map List", "", tr("MapList (*.mlist)"));

    if (filename.isEmpty())
        return;

    QFile file { filename };
    if (!file.open(QIODevice::WriteOnly))
    {
        QMessageBox box;
        box.setIcon(QMessageBox::Critical);
        box.setText("Couldn't open a file.");
        box.exec();
        return;
    }

    QJsonArray jArr;

    for(int i = 0; i < ui->mapSet->count(); ++i)
    {
        QListWidgetItem* item = ui->mapSet->item(i);
        jArr.append(item->checkState() == Qt::Checked ? true : false);
    }

    auto str = QJsonDocument { jArr }.toJson(QJsonDocument::Compact);
    file.write(str);
    file.close();
}

void MainWindow::on_playersContextMenu(const QPoint& point)
{
    if(ui->players->count() <= 0)
        return;

    if(ui->players->selectedItems().count() <= 0)
        return;

    QMenu contextMenu("Player Actions", this);

    QAction action1("Kick", this);
    connect(&action1, SIGNAL(triggered()), this, SLOT(on_player_kick()));
    contextMenu.addAction(&action1);

    QAction action2("Ban", this);
    connect(&action2, SIGNAL(triggered()), this, SLOT(on_player_ban()));
    contextMenu.addAction(&action2);

    contextMenu.setTitle("Player Actions");
    contextMenu.exec(ui->players->mapToGlobal(point));
}

void MainWindow::on_mapsContextMenu(const QPoint& point)
{
    QMenu contextMenu("Maplist Actions", this);

    QAction action1("Check all", this);
    connect(&action1, SIGNAL(triggered()), this, SLOT(on_mapsCheckAll()));
    contextMenu.addAction(&action1);

    QAction action2("Uncheck all", this);
    connect(&action2, SIGNAL(triggered()), this, SLOT(on_mapsUncheckAll()));
    contextMenu.addAction(&action2);

    contextMenu.setTitle("Maplist Actions");
    contextMenu.exec(ui->mapSet->mapToGlobal(point));
}

void MainWindow::on_mapsCheckAll()
{
    for(int i = 0; i < ui->mapSet->count(); ++i)
    {
        QListWidgetItem* item = ui->mapSet->item(i);
        item->setCheckState(Qt::Checked);
    }

    ui->comboBox->setCurrentIndex(8);
    queueEvent(PollData { 0, PollData::POLL_CLEAR_EXCLUDES });

    for(quint16 i = 0; i < ui->mapSet->count(); i++)
    {
        QListWidgetItem* item = ui->mapSet->item(i);

        if(item->checkState() == Qt::Unchecked)
            queueEvent(PollData { i , PollData::POLL_ADD_EXCLUDE });
    }
}

void MainWindow::on_mapsUncheckAll()
{
    for(int i = 0; i < ui->mapSet->count(); ++i)
    {
        QListWidgetItem* item = ui->mapSet->item(i);
        item->setCheckState(Qt::Unchecked);
    }

    ui->comboBox->setCurrentIndex(8);
    queueEvent(PollData { 0, PollData::POLL_CLEAR_EXCLUDES });

    for(quint16 i = 0; i < ui->mapSet->count(); i++)
    {
        QListWidgetItem* item = ui->mapSet->item(i);

        if(item->checkState() == Qt::Unchecked)
            queueEvent(PollData { i , PollData::POLL_ADD_EXCLUDE });
    }
}

void MainWindow::on_player_kick()
{
    for(auto it : ui->players->selectedItems())
    {
        auto pid = it->data(Qt::UserRole).toInt();

        queueEvent({ (uint16_t)pid, PollData::POLL_KICK });
    }
}

void MainWindow::on_player_ban()
{
    for(QListWidgetItem* it : ui->players->selectedItems())
    {
        auto pid = it->data(Qt::UserRole).toInt();

        queueEvent({ (uint16_t)pid, PollData::POLL_BAN });
    }
}

void MainWindow::addBan(QString name, QString ip)
{
    QListWidgetItem* item = new QListWidgetItem(ui->banList);
    item->setText(name);
    item->setData(Qt::UserRole, ip);
    ui->banList->addItem(item);
}

void MainWindow::on_banListDelete_clicked()
{
    if(ui->banList->count() <= 0)
        return;

    if(ui->banList->selectedItems().count() <= 0)
        return;

    for(QListWidgetItem* it : ui->banList->selectedItems())
    {
        auto ip = it->data(Qt::UserRole).toString();
        auto data = PollData { 0, PollData::POLL_UNBAN };

        const quint16* raw = reinterpret_cast<const quint16*>(ip.data());
        auto size = ip.length() * sizeof(const quint16);
        memcpy((void*)data.value2, (void*)raw, size);

        queueEvent(data);

        delete it;
        break;
    }
}


void MainWindow::on_backToLobby_clicked()
{
    queueEvent({ 0, PollData::POLL_BACKTOLOBBY });
}


void MainWindow::on_exeWin_clicked()
{
    queueEvent({ 0, PollData::POLL_EXEWIN });
}


void MainWindow::on_pushButton_2_clicked()
{
    queueEvent({ 0, PollData::POLL_SURVWIN });
}


void MainWindow::on_pushButton_clicked()
{
    queueEvent({ 0, PollData::POLL_PRACTICE });
}

