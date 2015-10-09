SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

--
-- Table structure for table `customcommands`
--

CREATE TABLE IF NOT EXISTS `customcommands` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `type` varchar(200) NOT NULL DEFAULT 'response',
  `permission` varchar(200) NOT NULL DEFAULT 'all',
  `channels` varchar(200) NOT NULL,
  `nicknames` varchar(200) NOT NULL,
  `trigger` varchar(200) NOT NULL,
  `response` text NOT NULL,
  `date_added` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4 AUTO_INCREMENT=0 ;
