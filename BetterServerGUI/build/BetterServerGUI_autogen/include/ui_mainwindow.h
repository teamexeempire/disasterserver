/********************************************************************************
** Form generated from reading UI file 'mainwindow.ui'
**
** Created by: Qt User Interface Compiler version 5.12.8
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_MAINWINDOW_H
#define UI_MAINWINDOW_H

#include <QtCore/QVariant>
#include <QtWidgets/QAction>
#include <QtWidgets/QApplication>
#include <QtWidgets/QComboBox>
#include <QtWidgets/QGridLayout>
#include <QtWidgets/QGroupBox>
#include <QtWidgets/QHBoxLayout>
#include <QtWidgets/QListWidget>
#include <QtWidgets/QMainWindow>
#include <QtWidgets/QMenu>
#include <QtWidgets/QMenuBar>
#include <QtWidgets/QPlainTextEdit>
#include <QtWidgets/QPushButton>
#include <QtWidgets/QStatusBar>
#include <QtWidgets/QTabWidget>
#include <QtWidgets/QVBoxLayout>
#include <QtWidgets/QWidget>

QT_BEGIN_NAMESPACE

class Ui_MainWindow
{
public:
    QAction *actionAbout_BetterServer;
    QAction *actionAbout_QT;
    QAction *actionExit;
    QWidget *main;
    QGridLayout *gridLayout;
    QTabWidget *tab_3;
    QWidget *page1;
    QHBoxLayout *horizontalLayout;
    QGroupBox *groupBox_2;
    QVBoxLayout *verticalLayout;
    QPlainTextEdit *logText;
    QGroupBox *horizontalGroupBox_2;
    QHBoxLayout *horizontalLayout_3;
    QPushButton *backToLobby;
    QPushButton *pushButton;
    QPushButton *exeWin;
    QPushButton *pushButton_2;
    QGroupBox *groupBox;
    QVBoxLayout *verticalLayout_2;
    QListWidget *players;
    QWidget *tab_4;
    QGridLayout *gridLayout_4;
    QHBoxLayout *horizontalLayout_2;
    QGroupBox *groupBox_17;
    QGridLayout *gridLayout_2;
    QHBoxLayout *horizontalLayout_9;
    QPushButton *mapSetOpen;
    QPushButton *mapSetSave;
    QListWidget *mapSet;
    QComboBox *comboBox;
    QGroupBox *groupBox_18;
    QGridLayout *gridLayout_3;
    QHBoxLayout *horizontalLayout_10;
    QPushButton *banListDelete;
    QListWidget *banList;
    QStatusBar *statusbar;
    QMenuBar *menubar;
    QMenu *menuFile;
    QMenu *menuAbout;

    void setupUi(QMainWindow *MainWindow)
    {
        if (MainWindow->objectName().isEmpty())
            MainWindow->setObjectName(QString::fromUtf8("MainWindow"));
        MainWindow->resize(767, 458);
        actionAbout_BetterServer = new QAction(MainWindow);
        actionAbout_BetterServer->setObjectName(QString::fromUtf8("actionAbout_BetterServer"));
        actionAbout_QT = new QAction(MainWindow);
        actionAbout_QT->setObjectName(QString::fromUtf8("actionAbout_QT"));
        actionExit = new QAction(MainWindow);
        actionExit->setObjectName(QString::fromUtf8("actionExit"));
        main = new QWidget(MainWindow);
        main->setObjectName(QString::fromUtf8("main"));
        gridLayout = new QGridLayout(main);
        gridLayout->setSpacing(2);
        gridLayout->setObjectName(QString::fromUtf8("gridLayout"));
        gridLayout->setContentsMargins(2, 2, 2, 0);
        tab_3 = new QTabWidget(main);
        tab_3->setObjectName(QString::fromUtf8("tab_3"));
        tab_3->setTabPosition(QTabWidget::North);
        tab_3->setTabShape(QTabWidget::Rounded);
        page1 = new QWidget();
        page1->setObjectName(QString::fromUtf8("page1"));
        horizontalLayout = new QHBoxLayout(page1);
        horizontalLayout->setObjectName(QString::fromUtf8("horizontalLayout"));
        horizontalLayout->setContentsMargins(4, -1, 4, -1);
        groupBox_2 = new QGroupBox(page1);
        groupBox_2->setObjectName(QString::fromUtf8("groupBox_2"));
        groupBox_2->setFlat(true);
        verticalLayout = new QVBoxLayout(groupBox_2);
        verticalLayout->setSpacing(0);
        verticalLayout->setObjectName(QString::fromUtf8("verticalLayout"));
        verticalLayout->setContentsMargins(2, 9, 2, 0);
        logText = new QPlainTextEdit(groupBox_2);
        logText->setObjectName(QString::fromUtf8("logText"));
        logText->setFrameShape(QFrame::Box);
        logText->setReadOnly(true);
        logText->setCenterOnScroll(true);

        verticalLayout->addWidget(logText);

        horizontalGroupBox_2 = new QGroupBox(groupBox_2);
        horizontalGroupBox_2->setObjectName(QString::fromUtf8("horizontalGroupBox_2"));
        horizontalLayout_3 = new QHBoxLayout(horizontalGroupBox_2);
        horizontalLayout_3->setObjectName(QString::fromUtf8("horizontalLayout_3"));
        backToLobby = new QPushButton(horizontalGroupBox_2);
        backToLobby->setObjectName(QString::fromUtf8("backToLobby"));

        horizontalLayout_3->addWidget(backToLobby);

        pushButton = new QPushButton(horizontalGroupBox_2);
        pushButton->setObjectName(QString::fromUtf8("pushButton"));

        horizontalLayout_3->addWidget(pushButton);

        exeWin = new QPushButton(horizontalGroupBox_2);
        exeWin->setObjectName(QString::fromUtf8("exeWin"));

        horizontalLayout_3->addWidget(exeWin);

        pushButton_2 = new QPushButton(horizontalGroupBox_2);
        pushButton_2->setObjectName(QString::fromUtf8("pushButton_2"));

        horizontalLayout_3->addWidget(pushButton_2);


        verticalLayout->addWidget(horizontalGroupBox_2);


        horizontalLayout->addWidget(groupBox_2);

        groupBox = new QGroupBox(page1);
        groupBox->setObjectName(QString::fromUtf8("groupBox"));
        groupBox->setAlignment(Qt::AlignLeading|Qt::AlignLeft|Qt::AlignVCenter);
        groupBox->setFlat(true);
        verticalLayout_2 = new QVBoxLayout(groupBox);
        verticalLayout_2->setSpacing(0);
        verticalLayout_2->setObjectName(QString::fromUtf8("verticalLayout_2"));
        verticalLayout_2->setContentsMargins(2, -1, 2, 0);
        players = new QListWidget(groupBox);
        players->setObjectName(QString::fromUtf8("players"));

        verticalLayout_2->addWidget(players);


        horizontalLayout->addWidget(groupBox);

        horizontalLayout->setStretch(0, 3);
        horizontalLayout->setStretch(1, 1);
        tab_3->addTab(page1, QString());
        tab_4 = new QWidget();
        tab_4->setObjectName(QString::fromUtf8("tab_4"));
        gridLayout_4 = new QGridLayout(tab_4);
        gridLayout_4->setObjectName(QString::fromUtf8("gridLayout_4"));
        horizontalLayout_2 = new QHBoxLayout();
        horizontalLayout_2->setObjectName(QString::fromUtf8("horizontalLayout_2"));
        groupBox_17 = new QGroupBox(tab_4);
        groupBox_17->setObjectName(QString::fromUtf8("groupBox_17"));
        gridLayout_2 = new QGridLayout(groupBox_17);
        gridLayout_2->setObjectName(QString::fromUtf8("gridLayout_2"));
        gridLayout_2->setSizeConstraint(QLayout::SetDefaultConstraint);
        horizontalLayout_9 = new QHBoxLayout();
        horizontalLayout_9->setObjectName(QString::fromUtf8("horizontalLayout_9"));
        mapSetOpen = new QPushButton(groupBox_17);
        mapSetOpen->setObjectName(QString::fromUtf8("mapSetOpen"));

        horizontalLayout_9->addWidget(mapSetOpen);

        mapSetSave = new QPushButton(groupBox_17);
        mapSetSave->setObjectName(QString::fromUtf8("mapSetSave"));

        horizontalLayout_9->addWidget(mapSetSave);


        gridLayout_2->addLayout(horizontalLayout_9, 2, 0, 1, 1);

        mapSet = new QListWidget(groupBox_17);
        mapSet->setObjectName(QString::fromUtf8("mapSet"));
        mapSet->setEditTriggers(QAbstractItemView::NoEditTriggers);
        mapSet->setSelectionMode(QAbstractItemView::NoSelection);
        mapSet->setSelectionBehavior(QAbstractItemView::SelectColumns);
        mapSet->setSelectionRectVisible(true);

        gridLayout_2->addWidget(mapSet, 1, 0, 1, 1);

        comboBox = new QComboBox(groupBox_17);
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->addItem(QString());
        comboBox->setObjectName(QString::fromUtf8("comboBox"));
        comboBox->setModelColumn(0);

        gridLayout_2->addWidget(comboBox, 0, 0, 1, 1);


        horizontalLayout_2->addWidget(groupBox_17);

        groupBox_18 = new QGroupBox(tab_4);
        groupBox_18->setObjectName(QString::fromUtf8("groupBox_18"));
        gridLayout_3 = new QGridLayout(groupBox_18);
        gridLayout_3->setObjectName(QString::fromUtf8("gridLayout_3"));
        horizontalLayout_10 = new QHBoxLayout();
        horizontalLayout_10->setSpacing(6);
        horizontalLayout_10->setObjectName(QString::fromUtf8("horizontalLayout_10"));
        horizontalLayout_10->setContentsMargins(30, -1, 30, -1);
        banListDelete = new QPushButton(groupBox_18);
        banListDelete->setObjectName(QString::fromUtf8("banListDelete"));

        horizontalLayout_10->addWidget(banListDelete);


        gridLayout_3->addLayout(horizontalLayout_10, 1, 1, 1, 1);

        banList = new QListWidget(groupBox_18);
        banList->setObjectName(QString::fromUtf8("banList"));
        banList->setResizeMode(QListView::Fixed);

        gridLayout_3->addWidget(banList, 0, 1, 1, 1);


        horizontalLayout_2->addWidget(groupBox_18);

        horizontalLayout_2->setStretch(0, 2);
        horizontalLayout_2->setStretch(1, 1);

        gridLayout_4->addLayout(horizontalLayout_2, 0, 0, 1, 1);

        tab_3->addTab(tab_4, QString());

        gridLayout->addWidget(tab_3, 0, 0, 1, 1);

        MainWindow->setCentralWidget(main);
        statusbar = new QStatusBar(MainWindow);
        statusbar->setObjectName(QString::fromUtf8("statusbar"));
        MainWindow->setStatusBar(statusbar);
        menubar = new QMenuBar(MainWindow);
        menubar->setObjectName(QString::fromUtf8("menubar"));
        menubar->setGeometry(QRect(0, 0, 767, 21));
        menuFile = new QMenu(menubar);
        menuFile->setObjectName(QString::fromUtf8("menuFile"));
        menuAbout = new QMenu(menubar);
        menuAbout->setObjectName(QString::fromUtf8("menuAbout"));
        MainWindow->setMenuBar(menubar);

        menubar->addAction(menuFile->menuAction());
        menubar->addAction(menuAbout->menuAction());
        menuFile->addAction(actionExit);
        menuAbout->addAction(actionAbout_BetterServer);
        menuAbout->addAction(actionAbout_QT);

        retranslateUi(MainWindow);

        tab_3->setCurrentIndex(0);


        QMetaObject::connectSlotsByName(MainWindow);
    } // setupUi

    void retranslateUi(QMainWindow *MainWindow)
    {
        MainWindow->setWindowTitle(QApplication::translate("MainWindow", "MainWindow", nullptr));
        actionAbout_BetterServer->setText(QApplication::translate("MainWindow", "About BetterServer", nullptr));
        actionAbout_QT->setText(QApplication::translate("MainWindow", "About Qt", nullptr));
        actionExit->setText(QApplication::translate("MainWindow", "Exit", nullptr));
        groupBox_2->setTitle(QApplication::translate("MainWindow", "Logs", nullptr));
        logText->setPlainText(QString());
        horizontalGroupBox_2->setTitle(QApplication::translate("MainWindow", "Control Panel", nullptr));
#ifndef QT_NO_TOOLTIP
        backToLobby->setToolTip(QApplication::translate("MainWindow", "<html><head/><body><p>Stop the ongoing game and return to the lobby immediately</p></body></html>", nullptr));
#endif // QT_NO_TOOLTIP
        backToLobby->setText(QApplication::translate("MainWindow", "Back To Lobby", nullptr));
#ifndef QT_NO_TOOLTIP
        pushButton->setToolTip(QApplication::translate("MainWindow", "<html><head/><body><p>Start practice mode</p></body></html>", nullptr));
#endif // QT_NO_TOOLTIP
        pushButton->setText(QApplication::translate("MainWindow", "Practice Mode", nullptr));
#ifndef QT_NO_TOOLTIP
        exeWin->setToolTip(QApplication::translate("MainWindow", "<html><head/><body><p>Force exe to win the game</p></body></html>", nullptr));
#endif // QT_NO_TOOLTIP
        exeWin->setText(QApplication::translate("MainWindow", "Force EXE win", nullptr));
#ifndef QT_NO_TOOLTIP
        pushButton_2->setToolTip(QApplication::translate("MainWindow", "<html><head/><body><p>Force survivors to win the game</p></body></html>", nullptr));
#endif // QT_NO_TOOLTIP
        pushButton_2->setText(QApplication::translate("MainWindow", "Force Survivors win", nullptr));
        groupBox->setTitle(QApplication::translate("MainWindow", "Players", nullptr));
        tab_3->setTabText(tab_3->indexOf(page1), QApplication::translate("MainWindow", "Main", nullptr));
        groupBox_17->setTitle(QApplication::translate("MainWindow", "Map sets", nullptr));
#ifndef QT_NO_TOOLTIP
        mapSetOpen->setToolTip(QApplication::translate("MainWindow", "Load map set from .mlist file", nullptr));
#endif // QT_NO_TOOLTIP
        mapSetOpen->setText(QApplication::translate("MainWindow", "Open", nullptr));
#ifndef QT_NO_TOOLTIP
        mapSetSave->setToolTip(QApplication::translate("MainWindow", "Save map set to .mlist file", nullptr));
#endif // QT_NO_TOOLTIP
        mapSetSave->setText(QApplication::translate("MainWindow", "Save", nullptr));
        comboBox->setItemText(0, QApplication::translate("MainWindow", "Default", nullptr));
        comboBox->setItemText(1, QApplication::translate("MainWindow", "Classic", nullptr));
        comboBox->setItemText(2, QApplication::translate("MainWindow", "Easy", nullptr));
        comboBox->setItemText(3, QApplication::translate("MainWindow", "Medium", nullptr));
        comboBox->setItemText(4, QApplication::translate("MainWindow", "Hard", nullptr));
        comboBox->setItemText(5, QApplication::translate("MainWindow", "Madness", nullptr));
        comboBox->setItemText(6, QApplication::translate("MainWindow", "Nightmare Universe", nullptr));
        comboBox->setItemText(7, QApplication::translate("MainWindow", "Spirits of Hell", nullptr));
        comboBox->setItemText(8, QApplication::translate("MainWindow", "Custom", nullptr));

#ifndef QT_NO_TOOLTIP
        comboBox->setToolTip(QApplication::translate("MainWindow", "Select map list preset", nullptr));
#endif // QT_NO_TOOLTIP
        groupBox_18->setTitle(QApplication::translate("MainWindow", "Ban List", nullptr));
#ifndef QT_NO_TOOLTIP
        banListDelete->setToolTip(QApplication::translate("MainWindow", "Revoke a ban", nullptr));
#endif // QT_NO_TOOLTIP
        banListDelete->setText(QApplication::translate("MainWindow", "Delete", nullptr));
        tab_3->setTabText(tab_3->indexOf(tab_4), QApplication::translate("MainWindow", "Options", nullptr));
        menuFile->setTitle(QApplication::translate("MainWindow", "File", nullptr));
        menuAbout->setTitle(QApplication::translate("MainWindow", "About", nullptr));
    } // retranslateUi

};

namespace Ui {
    class MainWindow: public Ui_MainWindow {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_MAINWINDOW_H
