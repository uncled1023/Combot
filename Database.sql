-- phpMyAdmin SQL Dump
-- version 4.0.0
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Apr 26, 2015 at 09:21 PM
-- Server version: 5.6.11-log
-- PHP Version: 5.4.15

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `combot`
--

-- --------------------------------------------------------

--
-- Table structure for table `channeljoins`
--

CREATE TABLE IF NOT EXISTS `channeljoins` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=77774 ;

-- --------------------------------------------------------

--
-- Table structure for table `channelkicks`
--

CREATE TABLE IF NOT EXISTS `channelkicks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `kicked_nick_id` int(11) NOT NULL,
  `reason` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=1442 ;

-- --------------------------------------------------------

--
-- Table structure for table `channelmessages`
--

CREATE TABLE IF NOT EXISTS `channelmessages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `message` text NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4 AUTO_INCREMENT=624233 ;

-- --------------------------------------------------------

--
-- Table structure for table `channelparts`
--

CREATE TABLE IF NOT EXISTS `channelparts` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=3446 ;

-- --------------------------------------------------------

--
-- Table structure for table `channelrules`
--

CREATE TABLE IF NOT EXISTS `channelrules` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `rule` text CHARACTER SET utf8mb4 NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=62 ;

-- --------------------------------------------------------

--
-- Table structure for table `channels`
--

CREATE TABLE IF NOT EXISTS `channels` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=96 ;

-- --------------------------------------------------------

--
-- Table structure for table `introductions`
--

CREATE TABLE IF NOT EXISTS `introductions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `channel_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `message` text CHARACTER SET utf8mb4 NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=609 ;

-- --------------------------------------------------------

--
-- Table structure for table `messages`
--

CREATE TABLE IF NOT EXISTS `messages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `sender_nick_id` int(11) NOT NULL,
  `message` text NOT NULL,
  `anonymous` tinyint(1) NOT NULL,
  `date_posted` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=719 ;

-- --------------------------------------------------------

--
-- Table structure for table `nickinfo`
--

CREATE TABLE IF NOT EXISTS `nickinfo` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nick_id` int(11) NOT NULL,
  `username` varchar(50) NOT NULL,
  `realname` varchar(50) NOT NULL,
  `host` varchar(100) NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Table structure for table `nicks`
--

CREATE TABLE IF NOT EXISTS `nicks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nickname` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=9134 ;

-- --------------------------------------------------------

--
-- Table structure for table `privatemessages`
--

CREATE TABLE IF NOT EXISTS `privatemessages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `message` text CHARACTER SET utf8mb4 NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=1185 ;

-- --------------------------------------------------------

--
-- Table structure for table `quits`
--

CREATE TABLE IF NOT EXISTS `quits` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `message` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=53934 ;

-- --------------------------------------------------------

--
-- Table structure for table `servers`
--

CREATE TABLE IF NOT EXISTS `servers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=3 ;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
