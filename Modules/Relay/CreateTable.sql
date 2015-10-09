SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

--
-- Table structure for table `relays`
--

CREATE TABLE IF NOT EXISTS `relays` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `nick_id` int(11) NOT NULL,
  `source` varchar(500) NOT NULL,
  `target` varchar(500) NOT NULL,
  `type` int(11) NOT NULL,
  `modes` varchar(250) NOT NULL,
  `date_added` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4 AUTO_INCREMENT=0 ;
